﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Blueprint.Properties {
    using System;


    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class Resources {

        private static global::System.Resources.ResourceManager resourceMan;

        private static global::System.Globalization.CultureInfo resourceCulture;

        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }

        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Blueprint.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }

        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to CreateNewElement should not be called due to overriding of IsElementName and CreateNewElement(string).
        /// </summary>
        public static string CachingConfigurationCollection_CreateNewElement_NotImplementedException_Message {
            get {
                return ResourceManager.GetString("CachingConfigurationCollection_CreateNewElement_NotImplementedException_Message", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Cannot resolve type of &apos;{0}&apos;..
        /// </summary>
        public static string CachingOptionsElement_Cannot_Resolve_Type {
            get {
                return ResourceManager.GetString("CachingOptionsElement_Cannot_Resolve_Type", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Cannot find any claim keys for the resource &apos;{0}&apos;..
        /// </summary>
        public static string ClaimKeyProvider_UnknownResourceException_Message {
            get {
                return ResourceManager.GetString("ClaimKeyProvider_UnknownResourceException_Message", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Please set at least one ClaimRequired attribute on the message &apos;{0}&apos;. If all users are allowed to send the message then use [MustBeAuthenticated]..
        /// </summary>
        public static string ClaimRequiredAttributeNotFoundException_Message {
            get {
                return ResourceManager.GetString("ClaimRequiredAttributeNotFoundException_Message", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Command executed with return code of &apos;{0}&apos;..
        /// </summary>
        public static string CommandBehaviourResult_Message {
            get {
                return ResourceManager.GetString("CommandBehaviourResult_Message", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to This attribute can only be assigned to string or DateTime property..
        /// </summary>
        public static string InFutureAttribute_IsValid_NotDateTime_Message {
            get {
                return ResourceManager.GetString("InFutureAttribute_IsValid_NotDateTime_Message", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to No command handler found for type &apos;{0}&apos;..
        /// </summary>
        public static string NoCommandHandlerFoundException_Message {
            get {
                return ResourceManager.GetString("NoCommandHandlerFoundException_Message", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to This attribute can only be assigned to collection property..
        /// </summary>
        public static string NotEmptyListAttribute_IsValid_NonEnumerableException_Message {
            get {
                return ResourceManager.GetString("NotEmptyListAttribute_IsValid_NonEnumerableException_Message", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to No query handler found for type &apos;{0}&apos;..
        /// </summary>
        public static string QueryHandlerBehaviour_InvokeQueryHandler_Cannot_Find_Handler {
            get {
                return ResourceManager.GetString("QueryHandlerBehaviour_InvokeQueryHandler_Cannot_Find_Handler", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Appending hashed environment header of &apos;{0}&apos; to email message..
        /// </summary>
        public static string SmtpClientEmailSender_EnvironmentHeader {
            get {
                return ResourceManager.GetString("SmtpClientEmailSender_EnvironmentHeader", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Validation failed for message of type &apos;{0}&apos;. Failures were: {1}.
        /// </summary>
        public static string ValidationException_Message {
            get {
                return ResourceManager.GetString("ValidationException_Message", resourceCulture);
            }
        }
    }
}