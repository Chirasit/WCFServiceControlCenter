﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace WinFormTestWcf.ServiceReference1 {
    using System.Runtime.Serialization;
    using System;
    
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="AfterLotEndEventArgs", Namespace="http://schemas.datacontract.org/2004/07/")]
    [System.SerializableAttribute()]
    public partial class AfterLotEndEventArgs : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private int JobIdField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private System.Nullable<int> JobSpecialFlowIdField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string LotJudgeField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string LotNoField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string McNoField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string OpNoField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public int JobId {
            get {
                return this.JobIdField;
            }
            set {
                if ((this.JobIdField.Equals(value) != true)) {
                    this.JobIdField = value;
                    this.RaisePropertyChanged("JobId");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<int> JobSpecialFlowId {
            get {
                return this.JobSpecialFlowIdField;
            }
            set {
                if ((this.JobSpecialFlowIdField.Equals(value) != true)) {
                    this.JobSpecialFlowIdField = value;
                    this.RaisePropertyChanged("JobSpecialFlowId");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false)]
        public string LotJudge {
            get {
                return this.LotJudgeField;
            }
            set {
                if ((object.ReferenceEquals(this.LotJudgeField, value) != true)) {
                    this.LotJudgeField = value;
                    this.RaisePropertyChanged("LotJudge");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string LotNo {
            get {
                return this.LotNoField;
            }
            set {
                if ((object.ReferenceEquals(this.LotNoField, value) != true)) {
                    this.LotNoField = value;
                    this.RaisePropertyChanged("LotNo");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string McNo {
            get {
                return this.McNoField;
            }
            set {
                if ((object.ReferenceEquals(this.McNoField, value) != true)) {
                    this.McNoField = value;
                    this.RaisePropertyChanged("McNo");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string OpNo {
            get {
                return this.OpNoField;
            }
            set {
                if ((object.ReferenceEquals(this.OpNoField, value) != true)) {
                    this.OpNoField = value;
                    this.RaisePropertyChanged("OpNo");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="ResultInfo", Namespace="http://schemas.datacontract.org/2004/07/")]
    [System.SerializableAttribute()]
    [System.Runtime.Serialization.KnownTypeAttribute(typeof(WinFormTestWcf.ServiceReference1.AfterLotEndResult))]
    public partial class ResultInfo : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string ErrorMessageField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private bool HasErrorField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string WarningMessageField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string ErrorMessage {
            get {
                return this.ErrorMessageField;
            }
            set {
                if ((object.ReferenceEquals(this.ErrorMessageField, value) != true)) {
                    this.ErrorMessageField = value;
                    this.RaisePropertyChanged("ErrorMessage");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public bool HasError {
            get {
                return this.HasErrorField;
            }
            set {
                if ((this.HasErrorField.Equals(value) != true)) {
                    this.HasErrorField = value;
                    this.RaisePropertyChanged("HasError");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string WarningMessage {
            get {
                return this.WarningMessageField;
            }
            set {
                if ((object.ReferenceEquals(this.WarningMessageField, value) != true)) {
                    this.WarningMessageField = value;
                    this.RaisePropertyChanged("WarningMessage");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="AfterLotEndResult", Namespace="http://schemas.datacontract.org/2004/07/")]
    [System.SerializableAttribute()]
    public partial class AfterLotEndResult : WinFormTestWcf.ServiceReference1.ResultInfo {
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="ServiceReference1.IServiceControlCenter")]
    public interface IServiceControlCenter {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IServiceControlCenter/DoWork", ReplyAction="http://tempuri.org/IServiceControlCenter/DoWorkResponse")]
        void DoWork();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IServiceControlCenter/DoWork", ReplyAction="http://tempuri.org/IServiceControlCenter/DoWorkResponse")]
        System.Threading.Tasks.Task DoWorkAsync();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IServiceControlCenter/AfterLotEnd", ReplyAction="http://tempuri.org/IServiceControlCenter/AfterLotEndResponse")]
        WinFormTestWcf.ServiceReference1.AfterLotEndResult AfterLotEnd(WinFormTestWcf.ServiceReference1.AfterLotEndEventArgs e);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IServiceControlCenter/AfterLotEnd", ReplyAction="http://tempuri.org/IServiceControlCenter/AfterLotEndResponse")]
        System.Threading.Tasks.Task<WinFormTestWcf.ServiceReference1.AfterLotEndResult> AfterLotEndAsync(WinFormTestWcf.ServiceReference1.AfterLotEndEventArgs e);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IServiceControlCenter/test", ReplyAction="http://tempuri.org/IServiceControlCenter/testResponse")]
        void test();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IServiceControlCenter/test", ReplyAction="http://tempuri.org/IServiceControlCenter/testResponse")]
        System.Threading.Tasks.Task testAsync();
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IServiceControlCenterChannel : WinFormTestWcf.ServiceReference1.IServiceControlCenter, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class ServiceControlCenterClient : System.ServiceModel.ClientBase<WinFormTestWcf.ServiceReference1.IServiceControlCenter>, WinFormTestWcf.ServiceReference1.IServiceControlCenter {
        
        public ServiceControlCenterClient() {
        }
        
        public ServiceControlCenterClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public ServiceControlCenterClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public ServiceControlCenterClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public ServiceControlCenterClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public void DoWork() {
            base.Channel.DoWork();
        }
        
        public System.Threading.Tasks.Task DoWorkAsync() {
            return base.Channel.DoWorkAsync();
        }
        
        public WinFormTestWcf.ServiceReference1.AfterLotEndResult AfterLotEnd(WinFormTestWcf.ServiceReference1.AfterLotEndEventArgs e) {
            return base.Channel.AfterLotEnd(e);
        }
        
        public System.Threading.Tasks.Task<WinFormTestWcf.ServiceReference1.AfterLotEndResult> AfterLotEndAsync(WinFormTestWcf.ServiceReference1.AfterLotEndEventArgs e) {
            return base.Channel.AfterLotEndAsync(e);
        }
        
        public void test() {
            base.Channel.test();
        }
        
        public System.Threading.Tasks.Task testAsync() {
            return base.Channel.testAsync();
        }
    }
}
