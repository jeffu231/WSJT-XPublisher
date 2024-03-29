﻿using System;
using System.Collections.Generic;
using System.Text;

namespace M0LTE.WsjtxUdpLib.Messages
{
    /*
     * SwitchConfiguration  In 14                     quint32
     *                         Id (unique key)        utf8
     *                         Configuration Name     utf8
     *
     *      The server  may send  this message at  any time.   The message
     *      specifies the name of the  configuration to switch to. The new
     *      configuration must exist.
     */

    public class SwitchConfigurationMessage : IWsjtxCommandMessage
    {
        public string Id { get; set; }
        public byte[] GetBytes() => throw new NotImplementedException();
    }
}
