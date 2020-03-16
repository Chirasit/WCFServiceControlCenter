using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

// NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IServiceControlCenter" in both code and config file together.
[ServiceContract]
public interface IServiceControlCenter
{
    [OperationContract]
    void DoWork();

    [OperationContract]
    AfterLotEndResult AfterLotEnd(AfterLotEndEventArgs e);

    [OperationContract]
    AddSpecialFlowResult AddSpecialFlow(AddSpecialFlowEventArgs e);

    [OperationContract]
    ItemCheckingResult JigToolChecking(ItemCheckingEventArgs e);
    [OperationContract]
    List<JigDataInfo> JigToolGetData(string mcNo, string lotNo);
}
