﻿using System;

namespace M0LTE.WsjtxUdpLib.Messages
{
    /*
     * Free Text     In        9
     *                         Id (unique key)        utf8
     *                         Text                   utf8
     *                         Send                   bool
     *
     *      This message  allows the server  to set the current  free text
     *      message content. Sending this  message with a non-empty "Text"
     *      field is equivalent to typing  a new message (old contents are
     *      discarded) in to  the WSJT-X free text message  field or "Tx5"
     *      field (both  are updated) and if  the "Send" flag is  set then
     *      clicking the "Now" radio button for the "Tx5" field if tab one
     *      is current or clicking the "Free  msg" radio button if tab two
     *      is current.
     *
     *      It is the responsibility of the  sender to limit the length of
     *      the  message   text  and   to  limit   it  to   legal  message
     *      characters. Despite this,  it may be difficult  for the sender
     *      to determine the maximum message length without reimplementing
     *      the complete message encoding protocol. Because of this is may
     *      be better  to allow any  reasonable message length and  to let
     *      the WSJT-X application encode and possibly truncate the actual
     *      on-air message.
     *
     *      If the  message text is  empty the  meaning of the  message is
     *      refined  to send  the  current free  text  unchanged when  the
     *      "Send" flag is set or to  clear the current free text when the
     *      "Send" flag is  unset.  Note that this API does  not include a
     *      command to  determine the  contents of  the current  free text
     *      message.
     */

    public class FreeTextMessage : IWsjtxCommandMessage
    {
        public string Id { get; set; }
        public byte[] GetBytes() => throw new NotImplementedException();
    }
}
