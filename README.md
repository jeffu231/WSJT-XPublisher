### WSJT Services

This provides some useful features for WSJT. 

## MQTT

It listens for the broadcast messages from WSJT and can publish messages to an MQTT broker for the status information. The broker information can
be set in the app settings file along with the root topic to publish to. Messages will take the form of {rootTopic}/wsjtx/{instance}/status/{field} with a payload
of the value. Instance is drawn from the naming when multiple instances are started. It can be enabled or disabled from the app settings.

## DxMaps

It also can publish spots to DxMaps in the proper format without needing to run WSJTAggregator. You do not need to run multiple instances on seperate UDP ports as this can process them all from the same standard port of 2237. The dxmaps server and port are configurable in the app settings, but default to the correct values. It can be enabled or disabled from the app settings.

## API

A configuration API is provided to allow for enabling or disabling the publishing services via RESTful calls. This can be useful to change their state without having to edit the config. Restarting the application restores the default setting in app settings. Swagger UI is available for the api: host:80/api/wsjtx/swagger

## Docker 

Docker files are provided for an example to run the services.

 
