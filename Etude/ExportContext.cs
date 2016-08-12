#region Namespaces
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.IO;
using Autodesk.Revit.DB;
using Autodesk.Revit.Utility;
using Newtonsoft.Json;
#endregion // Namespaces

namespace Etude
{
  // Done:
  // Check instance transformation
  // Support transparency
  // Add scaling for Theo [(0,0),(20000,20000)]
  // Implement the external application button
  // Implement element properties
  // Eliminate multiple materials 
  // Prompt user for output file name and location
  // Eliminate null element properties, i.e. useless 
  //     JSON userData entries
  // Todo:
  // Check for file size
  // Instance/type reuse

  public class ExportContext : IExportContext
  {
    /// <summary>
    /// Scale entire top level BIM object node in JSON
    /// output. A scale of 1.0 will output the model in 
    /// millimetres. Currently we scale it to decimetres
    /// so that a typical model has a chance of fitting 
    /// into a cube with side length 100, i.e. 10 metres.
    /// </summary>
    double _scale_bim = 1.0;

    /// <summary>
    /// Scale applied to each vertex in each individual 
    /// BIM element. This can be used to scale the model 
    /// down from millimetres to metres, e.g.
    /// Currently we stick with millimetres after all
    /// at this level.
    /// </summary>
    double _scale_vertex = 1.0;

    /// <summary>
    /// If true, switch Y and Z coordinate 
    /// and flip X to negative to convert from
    /// Revit coordinate system to standard 3d
    /// computer graphics coordinate system with
    /// Z pointing out of screen, X towards right,
    /// Y up.
    /// </summary>
    bool _switch_coordinates = true;

    #region VertexLookupXyz
    /// <summary>
    /// A vertex lookup class to eliminate 
    /// duplicate vertex definitions.
    /// </summary>
    class VertexLookupXyz : Dictionary<XYZ, int>
    {
      #region XyzEqualityComparer
      /// <summary>
      /// Define equality for Revit XYZ points.
      /// Very rough tolerance, as used by Revit itself.
      /// </summary>
      class XyzEqualityComparer : IEqualityComparer<XYZ>
      {
        const double _sixteenthInchInFeet
          = 1.0 / ( 16.0 * 12.0 );

        public bool Equals( XYZ p, XYZ q )
        {
          return p.IsAlmostEqualTo( q,
            _sixteenthInchInFeet );
        }

        public int GetHashCode( XYZ p )
        {
          return Util.PointString( p ).GetHashCode();
        }
      }
      #endregion // XyzEqualityComparer

      public VertexLookupXyz()
        : base( new XyzEqualityComparer() )
      {
      }

      /// <summary>
      /// Return the index of the given vertex,
      /// adding a new entry if required.
      /// </summary>
      public int AddVertex( XYZ p )
      {
        return ContainsKey( p )
          ? this[p]
          : this[p] = Count;
      }
    }
    #endregion // VertexLookupXyz

    #region VertexLookupInt
    /// <summary>
    /// An integer-based 3D point class.
    /// </summary>
    class PointInt : IComparable<PointInt>
    {
      public long X { get; set; }
      public long Y { get; set; }
      public long Z { get; set; }

      //public PointInt( int x, int y, int z )
      //{
      //  X = x;
      //  Y = y;
      //  Z = z;
      //}

      /// <summary>
      /// Consider a Revit length zero 
      /// if is smaller than this.
      /// </summary>
      const double _eps = 1.0e-9;

      /// <summary>
      /// Conversion factor from feet to millimetres.
      /// </summary>
      const double _feet_to_mm = 25.4 * 12;

      /// <summary>
      /// Conversion a given length value 
      /// from feet to millimetre.
      /// </summary>
      static long ConvertFeetToMillimetres( double d )
      {
        if( 0 < d )
        {
          return _eps > d
            ? 0
            : (long) ( _feet_to_mm * d + 0.5 );

        }
        else
        {
          return _eps > -d
            ? 0
            : (long) ( _feet_to_mm * d - 0.5 );

        }
      }

      public PointInt( XYZ p, bool switch_coordinates )
      {
        X = ConvertFeetToMillimetres( p.X );
        Y = ConvertFeetToMillimetres( p.Y );
        Z = ConvertFeetToMillimetres( p.Z );

        if( switch_coordinates )
        {
          X = -X;
          long tmp = Y;
          Y = Z;
          Z = tmp;
        }
      }

      public int CompareTo( PointInt a )
      {
        long d = X - a.X;

        if( 0 == d )
        {
          d = Y - a.Y;

          if( 0 == d )
          {
            d = Z - a.Z;
          }
        }
        return ( 0 == d ) ? 0 : ( ( 0 < d ) ? 1 : -1 );
      }
    }

    /// <summary>
    /// A vertex lookup class to eliminate 
    /// duplicate vertex definitions.
    /// </summary>
    class VertexLookupInt : Dictionary<PointInt, int>
    {
      #region PointIntEqualityComparer
      /// <summary>
      /// Define equality for integer-based PointInt.
      /// </summary>
      class PointIntEqualityComparer : IEqualityComparer<PointInt>
      {
        public bool Equals( PointInt p, PointInt q )
        {
          return 0 == p.CompareTo( q );
        }

        public int GetHashCode( PointInt p )
        {
          return ( p.X.ToString()
            + "," + p.Y.ToString()
            + "," + p.Z.ToString() )
            .GetHashCode();
        }
      }
      #endregion // PointIntEqualityComparer

      public VertexLookupInt()
        : base( new PointIntEqualityComparer() )
      {
      }

      /// <summary>
      /// Return the index of the given vertex,
      /// adding a new entry if required.
      /// </summary>
      public int AddVertex( PointInt p )
      {
        return ContainsKey( p )
          ? this[p]
          : this[p] = Count;
      }
    }
    #endregion // VertexLookupInt

    Document _doc;
    string _filename;
    Container _container;
    Dictionary<string, Material> _materials;
    Dictionary<string, Model> _objects;
    Dictionary<string, Geometry> _geometries;

    Model _currentElement;

    // Keyed on material uid to handle several materials per element:

    Dictionary<string, Model> _currentObject;
    Dictionary<string, Geometry> _currentGeometry;
    Dictionary<string, VertexLookupInt> _vertices;

    Stack<ElementId> _elementStack = new Stack<ElementId>();
    Stack<Transform> _transformationStack = new Stack<Transform>();

    string _currentMaterialUid;

    public string myjs = null;

    Model CurrentObjectPerMaterial
    {
      get
      {
        return _currentObject[_currentMaterialUid];
      }
    }

    Geometry CurrentGeometryPerMaterial
    {
      get
      {
        return _currentGeometry[_currentMaterialUid];
      }
    }

    VertexLookupInt CurrentVerticesPerMaterial
    {
      get
      {
        return _vertices[_currentMaterialUid];
      }
    }

    Transform CurrentTransform
    {
      get
      {
        return _transformationStack.Peek();
      }
    }

    public override string ToString()
    {
      return myjs;
    }

    /// <summary>
    /// Set the current material
    /// </summary>
    void SetCurrentMaterial( string uidMaterial )
    {
      if( !_materials.ContainsKey( uidMaterial ) )
      {
                Autodesk.Revit.DB.Material material = _doc.GetElement(
          uidMaterial) as Autodesk.Revit.DB.Material;

        Material m
          = new Material();

        //m.metadata = new EtudeMaterialMetadata();
        //m.metadata.type = "material";
        //m.metadata.version = 4.2;
        //m.metadata.generator = "Etude 2015.0.0.0";

        m.UUID = uidMaterial;
        m.Name = material.Name;
        m.Type = "MeshPhongMaterial";
        m.Color = Util.ColorToInt( material.Color );
        m.Ambient = m.Color;
        m.Emissive = 0;
        m.Specular = m.Color;
        m.Shininess = 1; // todo: does this need scaling to e.g. [0,100]?
        m.Opacity = 0.01 * (double) ( 100 - material.Transparency ); // Revit has material.Transparency in [0,100], three.js expects opacity in [0.0,1.0]
        m.Transparent = 0 < material.Transparency;
        m.Wireframe = false;

        _materials.Add( uidMaterial, m );
      }
      _currentMaterialUid = uidMaterial;

      string uid_per_material = _currentElement.UUID + "-" + uidMaterial;

      if( !_currentObject.ContainsKey( uidMaterial ) )
      {
        Debug.Assert( !_currentGeometry.ContainsKey( uidMaterial ), "expected same keys in both" );

        _currentObject.Add( uidMaterial, new Model() );
        CurrentObjectPerMaterial.Name = _currentElement.Name;
        CurrentObjectPerMaterial.Geometry = uid_per_material;
        CurrentObjectPerMaterial.Material = _currentMaterialUid;
        CurrentObjectPerMaterial.Matrix = new double[] { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 };
        CurrentObjectPerMaterial.Type = "Mesh";
        CurrentObjectPerMaterial.UUID = uid_per_material;
      }

      if( !_currentGeometry.ContainsKey( uidMaterial ) )
      {
        _currentGeometry.Add( uidMaterial, new Geometry() );
        CurrentGeometryPerMaterial.UUID = uid_per_material;
        CurrentGeometryPerMaterial.Type = "Geometry";
        CurrentGeometryPerMaterial.Data = new GeometryData();
        CurrentGeometryPerMaterial.Data.Faces = new List<int>();
        CurrentGeometryPerMaterial.Data.Vertices = new List<double>();
        CurrentGeometryPerMaterial.Data.Normals = new List<double>();
        CurrentGeometryPerMaterial.Data.UVs = new List<double>();
        CurrentGeometryPerMaterial.Data.Visible = true;
        CurrentGeometryPerMaterial.Data.CastShadow = true;
        CurrentGeometryPerMaterial.Data.ReceiveShadow = false;
        CurrentGeometryPerMaterial.Data.DoubleSided = true;
        CurrentGeometryPerMaterial.Data.Scale = 1.0;
      }

      if( !_vertices.ContainsKey( uidMaterial ) )
      {
        _vertices.Add( uidMaterial, new VertexLookupInt() );
      }
    }

    public ExportContext( Document document, string filename )
    {
      _doc = document;
      _filename = filename;
    }

    public bool Start()
    {
      _materials = new Dictionary<string, Material>();
      _geometries = new Dictionary<string, Geometry>();
      _objects = new Dictionary<string, Model>();

      _transformationStack.Push( Transform.Identity );

      _container = new Container();

      _container.Metadata = new Metadata();
      _container.Metadata.Type = "Object";
      _container.Metadata.Version = 4.3;
      _container.Metadata.Generator = "Etude Revit Etude exporter";
      _container.Geometries = new List<Geometry>();

      _container.Object = new Model();
      _container.Object.UUID = _doc.ActiveView.UniqueId;
      _container.Object.Name = "BIM " + _doc.Title;
      _container.Object.Type = "Scene";

      // Scale entire BIM from millimetres to metres.

      _container.Object.Matrix = new double[] { 
        _scale_bim, 0, 0, 0, 
        0, _scale_bim, 0, 0, 
        0, 0, _scale_bim, 0, 
        0, 0, 0, _scale_bim };

      return true;
    }

    public void Finish()
    {
      // Finish populating scene

      _container.Materials = _materials.Values.ToList();

      _container.Geometries = _geometries.Values.ToList();

      _container.Object.Children = _objects.Values.ToList();

      // Serialise scene

      //using( FileStream stream
      //  = File.OpenWrite( filename ) )
      //{
      //  DataContractJsonSerializer serialiser
      //    = new DataContractJsonSerializer(
      //      typeof( EtudeContainer ) );
      //  serialiser.WriteObject( stream, _container );
      //}

      JsonSerializerSettings settings
        = new JsonSerializerSettings();

      settings.NullValueHandling = NullValueHandling.Ignore;

      Formatting formatting
        = UserSettings.JsonIndented
          ? Formatting.Indented
          : Formatting.None;

      myjs = JsonConvert.SerializeObject(
        _container, formatting, settings );

      File.WriteAllText( _filename, myjs );

#if USE_DYNAMIC_JSON
      // This saves the whole hassle of explicitly 
      // defining a whole hierarchy of C# classes
      // to serialise to JSON - do it all on the 
      // fly instead.

      // https://github.com/Etude/GHEtude/blob/master/GHEtude/GHEtude/Etude_geometry.cs

      dynamic jason = new ExpandoObject();

      //populate object properties

      jason.geometry = new ExpandoObject();
      jason.groups = new object[0];
      jason.material = matName;
      jason.position = new object[3];
      jason.position[0] = 0; jason.position[1] = 0; jason.position[2] = 0;
      jason.rotation = new object[3];
      jason.rotation[0] = 0; jason.rotation[1] = 0; jason.rotation[2] = 0;
      jason.quaternion = new object[4];
      jason.quaternion[0] = 0; jason.quaternion[1] = 0; jason.quaternion[2] = 0; jason.quaternion[3] = 0;
      jason.scale = new object[3];
      jason.scale[0] = 1; jason.scale[1] = 1; jason.scale[2] = 1;
      jason.visible = true;
      jason.castShadow = true;
      jason.receiveShadow = false;
      jason.doubleSided = true;


      //populate geometry object
      jason.geometry.metadata = new ExpandoObject();
      jason.geometry.metadata.version = 3.1;
      jason.geometry.metadata.generatedBy = "Etude Revit Etude exporter";
      jason.geometry.metadata.vertices = mesh.Vertices.Count;
      jason.geometry.metadata.faces = mesh.Faces.Count;
      jason.geometry.metadata.normals = 0;
      jason.geometry.metadata.colors = 0;
      jason.geometry.metadata.uvs = 0;
      jason.geometry.metadata.materials = 0;
      jason.geometry.metadata.morphTargets = 0;
      jason.geometry.metadata.bones = 0;

      jason.geometry.scale = 1.000;
      jason.geometry.materials = new object[0];
      jason.geometry.vertices = new object[mesh.Vertices.Count * 3];
      jason.geometry.morphTargets = new object[0];
      jason.geometry.normals = new object[0];
      jason.geometry.colors = new object[0];
      jason.geometry.uvs = new object[0];
      jason.geometry.faces = new object[mesh.Faces.Count * 3];
      jason.geometry.bones = new object[0];
      jason.geometry.skinIndices = new object[0];
      jason.geometry.skinWeights = new object[0];
      jason.geometry.animation = new ExpandoObject();

      //populate vertices
      int counter = 0;
      int i = 0;
      foreach( var v in mesh.Vertices )
      {
        jason.geometry.vertices[counter++] = mesh.Vertices[i].X;
        jason.geometry.vertices[counter++] = mesh.Vertices[i].Y;
        jason.geometry.vertices[counter++] = mesh.Vertices[i].Z;
        i++;
      }

      //populate faces
      counter = 0;
      i = 0;
      foreach( var f in mesh.Faces )
      {
        jason.geometry.faces[counter++] = mesh.Faces[i].A;
        jason.geometry.faces[counter++] = mesh.Faces[i].B;
        jason.geometry.faces[counter++] = mesh.Faces[i].C;
        i++;
      }

      return JsonConvert.SerializeObject( jason );
#endif // USE_DYNAMIC_JSON
    }

    public void OnPolymesh( PolymeshTopology polymesh )
    {
      //Debug.WriteLine( string.Format(
      //  "    OnPolymesh: {0} points, {1} facets, {2} normals {3}",
      //  polymesh.NumberOfPoints,
      //  polymesh.NumberOfFacets,
      //  polymesh.NumberOfNormals,
      //  polymesh.DistributionOfNormals ) );

      IList<XYZ> pts = polymesh.GetPoints();

      Transform t = CurrentTransform;

      pts = pts.Select( p => t.OfPoint( p ) ).ToList();

      int v1, v2, v3;

      foreach( PolymeshFacet facet
        in polymesh.GetFacets() )
      {
        //Debug.WriteLine( string.Format(
        //  "      {0}: {1} {2} {3}", i++,
        //  facet.V1, facet.V2, facet.V3 ) );

        v1 = CurrentVerticesPerMaterial.AddVertex( new PointInt(
          pts[facet.V1], _switch_coordinates ) );

        v2 = CurrentVerticesPerMaterial.AddVertex( new PointInt(
          pts[facet.V2], _switch_coordinates ) );

        v3 = CurrentVerticesPerMaterial.AddVertex( new PointInt(
          pts[facet.V3], _switch_coordinates ) );

        CurrentGeometryPerMaterial.Data.Faces.Add( 0 );
        CurrentGeometryPerMaterial.Data.Faces.Add( v1 );
        CurrentGeometryPerMaterial.Data.Faces.Add( v2 );
        CurrentGeometryPerMaterial.Data.Faces.Add( v3 );
      }
    }

    public void OnMaterial( MaterialNode node )
    {
      //Debug.WriteLine( "     --> On Material: " 
      //  + node.MaterialId + ": " + node.NodeName );

      // OnMaterial method can be invoked for every 
      // single out-coming mesh even when the material 
      // has not actually changed. Thus it is usually
      // beneficial to store the current material and 
      // only get its attributes when the material 
      // actually changes.

      ElementId id = node.MaterialId;

      if( ElementId.InvalidElementId != id )
      {
        Element m = _doc.GetElement( node.MaterialId );
        SetCurrentMaterial( m.UniqueId );
      }
      else
      {
        //string uid = Guid.NewGuid().ToString();

        // Generate a GUID based on colour, 
        // transparency, etc. to avoid duplicating
        // non-element material definitions.

        var iColor = Util.ColorToInt( node.Color );

        var uid = $"MaterialNode_{iColor}_{Util.RealString(node.Transparency*100)}";

        if( !_materials.ContainsKey( uid ) )
        {
            var m = new Material
                {
                    UUID = uid,
                    Type = "MeshPhongMaterial",
                    Color = iColor,
                    Ambient = iColor,
                    Emissive = 0,
                    Specular = iColor,
                    Shininess = node.Glossiness,
                    Opacity = 1 - node.Transparency,
                    Transparent = 0.0 < node.Transparency,
                    Wireframe = false
                };
                    
          _materials.Add( uid, m );
        }
        SetCurrentMaterial( uid );
      }
    }

    public bool IsCanceled()
    {
      // This method is invoked many 
      // times during the export process.

      return false;
    }

    public void OnRPC( RPCNode node )
    {
      Debug.WriteLine( "OnRPC: " + node.NodeName );
      Asset asset = node.GetAsset();
      Debug.WriteLine( "OnRPC: Asset:"
        + ( ( asset != null ) ? asset.Name : "Null" ) );
    }

    public RenderNodeAction OnViewBegin( ViewNode node )
    {
      Debug.WriteLine( "OnViewBegin: "
        + node.NodeName + "(" + node.ViewId.IntegerValue
        + "): LOD: " + node.LevelOfDetail );

      return RenderNodeAction.Proceed;
    }

    public void OnViewEnd( ElementId elementId )
    {
      Debug.WriteLine( "OnViewEnd: Id: " + elementId.IntegerValue );
      // Note: This method is invoked even for a view that was skipped.
    }

    public RenderNodeAction OnElementBegin(
      ElementId elementId )
    {
      var e = _doc.GetElement( elementId );
      var uid = e.UniqueId;

      Debug.WriteLine($"OnElementBegin: id {elementId.IntegerValue} category {e.Category.Name} name {e.Name}");

      if( _objects.ContainsKey( uid ) )
      {
        Debug.WriteLine( "\r\n*** Duplicate element!\r\n" );
        return RenderNodeAction.Skip;
      }

      if( null == e.Category )
      {
        Debug.WriteLine( "\r\n*** Non-category element!\r\n" );
        return RenderNodeAction.Skip;
      }

      _elementStack.Push( elementId );

      ICollection<ElementId> idsMaterialGeometry = e.GetMaterialIds( false );
      ICollection<ElementId> idsMaterialPaint = e.GetMaterialIds( true );

      int n = idsMaterialGeometry.Count;

      if( 1 < n )
      {
          Debug.Print($"{Util.ElementDescription(e)} has {n} materials: {string.Join(", ", idsMaterialGeometry.Select(id => _doc.GetElement(id).Name))}");
      }

      // We handle a current element, which may either
      // be identical to the current object and have
      // one single current geometry or have 
      // multiple current child objects each with a 
      // separate current geometry.

        _currentElement = new Model
        {
            Name = Util.ElementDescription(e),
            Material = _currentMaterialUid,
            Matrix = new double[] {1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1},
            Type = "RevitElement",
            UUID = uid
        };


        _currentObject = new Dictionary<string, Model>();
      _currentGeometry = new Dictionary<string, Geometry>();
      _vertices = new Dictionary<string, VertexLookupInt>();

      if( e.Category?.Material != null )
      {
        SetCurrentMaterial( e.Category.Material.UniqueId );
      }

      return RenderNodeAction.Proceed;
    }

    public void OnElementEnd(
      ElementId id )
    {
      // Note: this method is invoked even for 
      // elements that were skipped.

      Element e = _doc.GetElement( id );
      string uid = e.UniqueId;

      Debug.WriteLine($"OnElementEnd: id {id.IntegerValue} category {e.Category.Name} name {e.Name}");

      if( _objects.ContainsKey( uid ) )
      {
        Debug.WriteLine( "\r\n*** Duplicate element!\r\n" );
        return;
      }

      if( null == e.Category )
      {
        Debug.WriteLine( "\r\n*** Non-category element!\r\n" );
        return;
      }

      List<string> materials = _vertices.Keys.ToList();

      int n = materials.Count;

      _currentElement.Children = new List<Model>( n );

      foreach( string material in materials )
      {
        Model obj = _currentObject[material];
        Geometry geo = _currentGeometry[material];

        foreach( KeyValuePair<PointInt, int> p in _vertices[material] )
        {
          geo.Data.Vertices.Add( _scale_vertex * p.Key.X );
          geo.Data.Vertices.Add( _scale_vertex * p.Key.Y );
          geo.Data.Vertices.Add( _scale_vertex * p.Key.Z );
        }
        obj.Geometry = geo.UUID;
        _geometries.Add( geo.UUID, geo );
        _currentElement.Children.Add( obj );
      }

      Dictionary<string, string> d
        = Util.GetElementProperties( e, true );

      _currentElement.UserData = d;

      // Add Revit element unique id to user data dict.

      _currentElement.UserData.Add( "revit_id", uid );

      _objects.Add( _currentElement.UUID, _currentElement );

      _elementStack.Pop();
    }

    public RenderNodeAction OnFaceBegin( FaceNode node )
    {
      // This method is invoked only if the 
      // custom exporter was set to include faces.

      //Debug.Assert( false, "we set exporter.IncludeFaces false" );
      //Debug.WriteLine( "  OnFaceBegin: " + node.NodeName );
      return RenderNodeAction.Proceed;
    }

    public void OnFaceEnd( FaceNode node )
    {
      // This method is invoked only if the 
      // custom exporter was set to include faces.

      //Debug.Assert( false, "we set exporter.IncludeFaces false" );
      //Debug.WriteLine( "  OnFaceEnd: " + node.NodeName );
      // Note: This method is invoked even for faces that were skipped.
    }

    public RenderNodeAction OnInstanceBegin( InstanceNode node )
    {
      Debug.WriteLine( "  OnInstanceBegin: " + node.NodeName + " symbol: " + node.GetSymbolId().IntegerValue );
      // This method marks the start of processing a family instance
      _transformationStack.Push( CurrentTransform.Multiply( node.GetTransform() ) );

      // We can either skip this instance or proceed with rendering it.
      return RenderNodeAction.Proceed;
    }

    public void OnInstanceEnd( InstanceNode node )
    {
      Debug.WriteLine( "  OnInstanceEnd: " + node.NodeName );
      // Note: This method is invoked even for instances that were skipped.
      _transformationStack.Pop();
    }

    public RenderNodeAction OnLinkBegin( LinkNode node )
    {
      Debug.WriteLine( "  OnLinkBegin: " + node.NodeName + " Document: " + node.GetDocument().Title + ": Id: " + node.GetSymbolId().IntegerValue );
      _transformationStack.Push( CurrentTransform.Multiply( node.GetTransform() ) );
      return RenderNodeAction.Proceed;
    }

    public void OnLinkEnd( LinkNode node )
    {
      Debug.WriteLine( "  OnLinkEnd: " + node.NodeName );
      // Note: This method is invoked even for instances that were skipped.
      _transformationStack.Pop();
    }

    public void OnLight( LightNode node )
    {
      Debug.WriteLine( "OnLight: " + node.NodeName );
      Asset asset = node.GetAsset();
      Debug.WriteLine( "OnLight: Asset:" + ( ( asset != null ) ? asset.Name : "Null" ) );
    }
  }
}
