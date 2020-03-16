using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

/// <summary>
/// Summary description for ItemDataInfo
/// </summary>
[DataContract]
public class ItemDataInfo : ResultInfo
{
    [DataMember]
    public int MyProperty { get; set; }
}