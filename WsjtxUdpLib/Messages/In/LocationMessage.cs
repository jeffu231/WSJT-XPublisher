using System;

namespace M0LTE.WsjtxUdpLib.Messages
{
    /*
     * Location       In       11
     *                         Id (unique key)        utf8
     *                         Location               utf8
     *
     *      This  message allows  the server  to set  the current  current
     *      geographical location  of operation. The supplied  location is
     *      not persistent but  is used as a  session lifetime replacement
     *      loction that overrides the Maidenhead  grid locater set in the
     *      application  settings.  The  intent  is to  allow an  external
     *      application  to  update  the  operating  location  dynamically
     *      during a mobile period of operation.
     *
     *      Currently  only Maidenhead  grid  squares  or sub-squares  are
     *      accepted, i.e.  4- or 6-digit  locators. Other formats  may be
     *      accepted in future.
     */

    public class LocationMessage : WsjtxMessage, IWsjtxCommandMessage
    {
        public string Id { get; set; }
        
        public string Locator { get; set; }

        public byte[] GetBytes()
        {
            return Qt.Concat(
                MAGIC_NUMBER,
                SCHEMA_VERSION,
                Qt.Encode((uint)MessageType.LOCATION_MESSAGE_TYPE),
                Qt.Encode(Id),
                Qt.Encode(Locator));
        }
    }
}
