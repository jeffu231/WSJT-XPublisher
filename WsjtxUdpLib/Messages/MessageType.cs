﻿namespace M0LTE.WsjtxUdpLib.Messages
{
    public enum MessageType
    {
        HEARTBEAT_MESSAGE_TYPE = 0,
        STATUS_MESSAGE_TYPE = 1,
        DECODE_MESSAGE_TYPE = 2,
        CLEAR_MESSAGE_TYPE = 3,
        QSO_LOGGED_MESSAGE_TYPE = 5,
        CLOSE_MESSAGE_TYPE = 6,
        WSPR_DECODE_MESSAGE_TYPE = 10,
        LOCATION_MESSAGE_TYPE = 11,
        LOGGED_ADIF_MESSAGE_TYPE = 12,
        HIGHLIGHT_CALLSIGN_MESSAGE_TYPE = 13
        
    }
}