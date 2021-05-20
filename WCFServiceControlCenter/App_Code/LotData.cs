using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

/// <summary>
/// Summary description for LotData
/// </summary>
[DataContract]
public class LotData
{
    [DataMember]
    public int GoodAdjust { get; set; }
    [DataMember]
    public int NgAdjust { get; set; }
    [DataMember]
    public int PcsPerWork { get; set; }
    [DataMember]
    public int GoodFrameAdjust { get; set; }
    [DataMember]
    public int NgFrameAdjust { get; set; }
    [DataMember]
    public int? PNashi { get; set; }
    [DataMember]
    public int? FrontNg { get; set; }
    [DataMember]
    public int? Marker { get; set; }
    [DataMember]
    public int? PNashi_Scrap { get; set; }
    [DataMember]
    public int? FrontNg_Scrap { get; set; }
    [DataMember]
    public int? Marker_Scrap { get; set; }
    [DataMember]
    public int? Qty_Scrap { get; set; }
    /// <summary>
    /// FrontNg+PNashi
    /// </summary>
    [DataMember]
    public int? OS_Scrap { get; set; }
}

