﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

// 
// This source code was auto-generated by xsd, Version=4.6.81.0.
// 

namespace ProNet
{
    /// <remarks/>
    [GeneratedCode("xsd", "4.6.81.0")]
    [Serializable()]
    [DebuggerStepThrough()]
    [DesignerCategory("code")]
    [XmlType(AnonymousType=true)]
    [XmlRoot(Namespace="", IsNullable=false)]
    public partial class Network {
    
        private NetworkProgrammer[] programmerField;
    
        /// <remarks/>
        [XmlElement("Programmer")]
        public NetworkProgrammer[] Programmer {
            get {
                return this.programmerField;
            }
            set {
                this.programmerField = value;
            }
        }
    }

    /// <remarks/>
    [GeneratedCode("xsd", "4.6.81.0")]
    [Serializable()]
    [DebuggerStepThrough()]
    [DesignerCategory("code")]
    [XmlType(AnonymousType=true)]
    public partial class NetworkProgrammer {
    
        private string[] recommendationsField;
    
        private string[] skillsField;
    
        private string nameField;
    
        /// <remarks/>
        [XmlArrayItem("Recommendation", IsNullable=false)]
        public string[] Recommendations {
            get {
                return this.recommendationsField;
            }
            set {
                this.recommendationsField = value;
            }
        }
    
        /// <remarks/>
        [XmlArrayItem("Skill", IsNullable=false)]
        public string[] Skills {
            get {
                return this.skillsField;
            }
            set {
                this.skillsField = value;
            }
        }
    
        /// <remarks/>
        [XmlAttribute()]
        public string name {
            get {
                return this.nameField;
            }
            set {
                this.nameField = value;
            }
        }
    }
}
