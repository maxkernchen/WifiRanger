# WifiRanger is a C# WPF project which calculates the approximate percentage of WiFi coverage for a given router and floor plan. 

It uses Walmart JSON Web Services to retrieve up to date prices and store links for a current list of 10 popular routers.

WiFi coverage is calculate by a modified free-space path loss formula which adds the natural log of the router's power to it. Some basic real world testing using simple Android applications proved this method to be fairly accurate. Below is the current formula WifiRanger uses to calculate the distance, in one direction, covered in meters.

![alt text](https://i.imgur.com/07flSrR.png)

Frequency is between 2400 - 5800 megahertz for most routers, dBm or decibel-milliwatts is usually -57 or -58 which is the lowest WiFi strength for reliable packet delivery. Power is different for every router, most of the time higher power = higher range.

WifiRanger is simple to use, you select a router by clicking on one in the list. This list of routers is sortable by Brand, Model, and Price. You can also search for a router's name or model using the search bar.

![alt text](https://i.imgur.com/53umn3Y.png)

Then the number of floors, approximate router location, and area in square feet or square meters is entered. All these fields are required.
The area must be between 1 and 10,000 to be accepted.

![alt text](https://i.imgur.com/HUyMUJj.png)
