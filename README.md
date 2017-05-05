## eDocumentReader
Speech interactive eDocument reader

## The contributors to the project:
* Dr. Trung Bui of Adobe
* Mr. Jia Huang of Stanford University
* Mr. Daichi Ito of Adobe
* Dr. Gavin Miller of Adobe
* Prof. Stanley Peters of Stanford University

## Minimum Requirements
1. Windows 7,8, or 10

2. At least 2 GHz of CPU

3. Minimum of 3 GB of RAM

4. Google chrome browser version 49 or later (https://www.google.com/chrome/browser/desktop/)

5. IIS Express 7.0 or later (http://www.iis.net/downloads)

6. A good noise cancelling headphone set


## Configuration
1. Open the applicationhost.config file in C:\Users\<username>\Documents\IISExpress\config with your favor editor. <username> is the name of your Windows account.

2. Under the <sites> section in the applicationhost.config file, add the following code right before the closing tag </sites>. You need to replace 12.45.678.9 with your computer's IP address, and change the physicalPath parameter to point to the directoy of the project. If you see other <site> already exist in config file, change the id number to previous site's id number and increment by one.
```xml
<site name="eDocumentReader" id="2" serverAutoStart="true">
   <application path="/"  applicationPool="Clr4IntegratedAppPool">
      <virtualDirectory path="/" physicalPath="C:\eDocumentReader_v3.1.0" />
   </application>
   <bindings>
      <binding protocol="http" bindingInformation=":8080:localhost" />
      <binding protocol="https" bindingInformation="*:44301:123.45.678.9" />
   </bindings>
```


## Getting started
1. Open the google chrome browser and enter http://<server_ip_address>:8080/Home/TheLittleFrog where <server_ip_address> is the Ip address of server (Note that you can also run the browser from different copy on the network by using https://<server_ip_address>:44301/Home/TheLittleFrog)
2. If you see the text "Please select a story" in the browser, congratulations! You've successfully connected to the server.

## License
Apache License 2.0
