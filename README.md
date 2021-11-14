# WifiRanger is a C# WPF project which calculates the approximate percentage of WiFi coverage for a given router and floor plan. 

It display routers currently for sale on walmart.com, and will show real-time price and rating information retrieved from a third-party web service API.
The real-time results may not be available on initial start of the application, they are fetched in a background thread and will be updated when complete.

WiFi coverage is calculate by a modified free-space path loss formula which adds the natural log of the router's power to it. Some basic real world testing using simple Android applications proved this method to be fairly accurate. Below is the current formula WifiRanger uses to calculate the distance, in one direction, covered in meters.

![alt text](https://i.imgur.com/07flSrR.png)

Frequency is between 2400 - 5800 megahertz for most routers, dBm or decibel-milliwatts is usually -57 or -58 which is the lowest WiFi strength for reliable packet delivery. Power is different for every router, most of the time higher power = higher range.

WifiRanger is simple to use, you select a router by clicking on one in the list. This list of routers is sortable by Brand, Model, Rating, and Price. You can also search for a router's name or model using the search bar.

![alt text](https://i.imgur.com/GHo5TF2.png)

Then the number of floors, approximate router location, and area in square feet or square meters is entered. All these fields are required.
The area must be between 1 and 10,000 to be accepted.

![alt text](https://i.imgur.com/HUyMUJj.png)

Finally, the results are diplayed along with the store link of the route near the top and a link to my website where there is an explaination on how these results are estimated. There also a start over button which sends the user back to starting list of routers.

![alt text](https://i.imgur.com/z9V1BNr.png)

**NOTES**: If there no internet connection router store pages and prices will not be available.

Calculating the coverage of a router is very difficult. This software is meant for **estimations** only.
