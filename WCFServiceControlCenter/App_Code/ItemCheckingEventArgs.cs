using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

/// <summary>
/// Summary description for ItemCheckingEventArgs
/// </summary>
[DataContract]
public class ItemCheckingEventArgs
{
    [DataMember]
    public string QrCode { get; set; }
    [DataMember]
    public string PreviousQrCode { get; set; }
    [DataMember]
    public string DeviceName { get; set; }
    [DataMember]
    public string PackageName { get; set; }
    [DataMember]
    public string TpCode { get; set; }
    [DataMember]
    public string LotNo { get; set; }
    [DataMember]
    public string FrameType { get; set; }
    [DataMember]
    public string McNo { get; set; }
    [DataMember]
    public int McId { get; set; }
    [DataMember]
    public string McType { get; set; }
    [DataMember]
    public string OpNo { get; set; }
    [DataMember]
    public int OpId { get; set; }
}