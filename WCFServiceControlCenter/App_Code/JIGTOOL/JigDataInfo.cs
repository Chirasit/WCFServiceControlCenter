using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

/// <summary>
/// Summary description for JigDataInfo
/// </summary>
[DataContract]
public class JigDataInfo
{
    [DataMember]
    public int Id { get; set; }
    [DataMember]
    public string Name { get; set; }
    [DataMember]
    public string ShortName { get; set; }
    [DataMember]
    public string Type { get; set; }
    [DataMember]
    public string ExpireValue { get; set; }
    [DataMember]
    public string Status { get; set; }
    [DataMember]
    public bool IsChange { get; set; }
    [DataMember]
    public bool IsPass { get; set; }
    [DataMember]
    public string Message_Thai { get; set; }
    [DataMember]
    public string Message_Eng { get; set; }
    [DataMember]
    public string Handling { get; set; }
    [DataMember]
    public string Warning { get; set; }
    [DataMember]
    public bool IsWarning { get; set; }
    [DataMember]
    public string BarCode { get; set; }
    [DataMember]
    public string SmallCode { get; set; }
    [DataMember]
    public string QrCodeByUser { get; set; }
    [DataMember]
    public string SubType { get; set; }
    [DataMember]
    public int Value { get; set; }
    [DataMember]
    public int ValuePerLot { get; set; }


}