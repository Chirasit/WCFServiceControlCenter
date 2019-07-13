using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

/// <summary>
/// Summary description for AddSpecialFlowEventArgs
/// </summary>
[DataContract]
public class AddSpecialFlowEventArgs
{
    [DataMember]
    public string LotNo { get; set; }
    [DataMember]
    public string OpNo { get; set; }
    [DataMember]
    public int? JobSpecialFlowId { get; set; }
    [DataMember]
    public int? JobId { get; set; }
    [DataMember]
    public int? NextJobId { get; set; }
    [DataMember]
    public bool? IsAddNow { get; set; }
    [DataMember]
    public bool? IsConfirmAddSpecialFlow { get; set; }

}