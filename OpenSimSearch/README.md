Add the following to your own OpenSim.ini file to make Search work:
```text
[Search]

;; URL to the Neverworld Search Server
SearchURL = https://app.neverworldgrid.com/v1/search/query.php

In addition to the above, the following two lines need to be added to the
[Modules] section of the OpenSim.ini file.

    LandServices = "RemoteLandServicesConnector"
    LandServiceInConnector = "true"
```
