﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PS.Mothership.Core.Common.Rellaid.Dto
{
    [DataContract]
    public class ValidUserInfoDto
    {
        [DataMember]
        public Guid UserGuid { get; set; }

        [DataMember]
        public string PhoneNumberOverride { get; set; }

        [DataMember]
        public string VoicemailNumber { get; set; }

        [DataMember]
        public string MonitorPhoneNumber { get; set; }

        [DataMember]
        public string Extension { get; set; }

        [DataMember]
        public List<CallLogItemDto> CallLog { get; set; }

        [DataMember]
        public string SipUserId { get; set; }

        [DataMember]
        public string SipPassword { get; set; }

        [DataMember]
        public string SipProxyUserId { get; set; }

        [DataMember]
        public string SipProxyPassword { get; set; }

        [DataMember]
        public List<InboundQueueSubscriptionDto> InboundQueueSubscriptions { get; set; }

        [DataMember]
        public long DepartmentKey { get; set; }

        [DataMember]
        public long DepartmentCategoryKey { get; set; }
    }
}
