#region Namespaces
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
#endregion // Namespaces

namespace Etude
{
  /// <summary>
  /// three.js object class, successor of EtudeScene.
  /// The structure and properties defined here were
  /// reverse engineered ftom JSON files exported 
  /// by the three.js and Etude editors.
  /// </summary>
  public class Container
  {
    [JsonProperty("metadata")]
    public Metadata Metadata { get; set; }
    [JsonProperty("object")]
    public Model Object { get; set; }
    [JsonProperty("geometries")]
    public List<Geometry> Geometries;
    [JsonProperty("materials")]
    public List<Material> Materials;
  }
}
