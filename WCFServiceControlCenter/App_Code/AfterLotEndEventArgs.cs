using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

/// <summary>
/// Summary description for AfterLotEndEventArgs
/// </summary>
[DataContract]
public class AfterLotEndEventArgs
{
    [DataMember]
    public int JobId { get; set; }
    [DataMember]
    public string LotNo { get; set; }
    [DataMember]
    public string McNo { get; set; }
    [DataMember]
    public string OpNo { get; set; }
    [DataMember]
    public string LotJudge { get; set; }
    [DataMember]
    public int? JobSpecialFlowId { get; set; }
    [DataMember]
    public int? PNashi { get; set; }
    [DataMember]
    public int? FrontNg { get; set; }
    [DataMember]
    public int? Marker { get; set; }
    [DataMember]
    public int? CutFrame { get; set; }

}
