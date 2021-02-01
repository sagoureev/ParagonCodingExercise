This is my first solution to the Paragon Intel Coding Challenge.

Due to the nature of this assignment (with an implied completion time of a few hours), as well as a rather broad scope of data available, there are still many more potential modifications to the algorithms involved (even potentially changing the entire approach to flight tracking with a new dataset). However, his solution is implemented with a rather diverse set of sample data as its baseline, so I am confident in the first solution.

# Background

After examining the data provided, I found some unexpected inconsistencies. Most notably - a large chunk of ADS-B events appear to have data missing. In some cases it is the Coordinate values themselves. In others - part or all of the readout data.


I was also happy to learn that is was real data. A friendly phone call to Delta helped confirm that. All of the aircraft identifiers match real, working aircraft, whose information can be accessed with Google search. This greatly helped in the design and implementation of my solution. The entire solution has a "real world" approach to the design.


# Assumptions
With real world data available, I decided to design my solution around what I would consider a good piece off sample data (described in the "Data Used" section below in detail). Eyeballing the sample data, I was able to determine the following:

* ADS-B data always appears in "chunks", where ADS-B events are typically broadcast every few seconds.
* These "chunks", given a small enough time frame, do not change much. There is only so much an aircraft can reasonably do in that time.
* With the above assumption - we can infer that on a small enough time frame - missing data will more than likely match previous data for that "chunk".
* ADS-B "blind spots" tend to occur over a desolate land mass or the ocean.
* Inversly - ADS-B seems to work rather well at airports.

Also, due to the nautre of the "real word" data set, we can also make the more assumptions:
* Planes go slow on the ground.
* Planes fly at high speeds.
* Given the info above - we can deduce that a plane is in the air or on the ground based on its speed.
* If we know a plane is on the ground and it goes past a certain speed - it is taking off.
* If we know a plane is in the air and it drops below a certain speed - it is landing.
* If a plane is taking off or landing, we can assume that it will continue to do so for some time.
* Since this solution is used to track flights - "good enough" data is better than "unknown" data. In a real world case we would rather know that a plane landed in the general area of a city rather than an "unknown" location. Of course accurate data is better. And all of the test data has accuracy in mind.


# Data Used
To implement a solution, I picked 3 airplanes based on the variety their real world characteristics and saw some interesting data. I also created thorough unit tests for events related to each of those 3 airplanes. That way - if changes to the implementation is needed - I quickly know if the previous data is affected. 
*  **AA09F4 - Gulfstream Aerospace G-IV** - a private aircraft owned by a cosmetics company. This is a good sample of how "business people" would travel. This aircraft had the shortest time between takeoffs, which i used as a baseline for some constants inside my solution.
* **A44D3F - Boeing 737-800** - A standard commercial airliner operated by Delta. Because it flies into major cities, it is useful to test the accuracy of my landing calculations. We know that a Delta airliner has no business being in a municipal airport, for example.
* **A30626 - McDonnell Douglas MD-11F** - a cargo jet operated by UPS. I was curious to see how cargo jets fly, and this turned out to have several interesting data points. First of all - it would disappear for hours - since it flew over the ocean. Second - it presented an interesting data point since it was the only jet that didn't perfectly signal at the airport (When in Hawaii). Still, by looking at the data, i was able to deduce that it was landing at Kona airport.


# Implementation
When implementing the solution, and given the time constraints - I decided to focus on getting the takeoff and landing accuracy as perfect as possible with the data above. The bulk of the work is done in **AirplaneEventProcessor.cs**  - a class dedicated to the processing of ADS-B signals for an individual aircraft. An aircraft can be either flying, grounded or unknown (a state entered when we first receive the airplane data and do not yet know enough about its state). The rest of the soluttion focuses on setting up the AirplaneEventProcessor class for an individual aircraft, collecting data to display to the user, and displaying it.

One of the open ended partsof the assignment was the implementation of the AirportCollection class - specifically the data structure used for it. While geographic indexing certainly has been done in C# before, implementing it would take a significant amount of time beyond the limitations of this assignment. To work around that I used a simple array of Airports with as many optimizations as I could. First - I instantiate it to its full size. Then I use a simple for loop and numeric index lookup (faster than other looping methods), I keep the calls to AirportCollection to a minimum, and
last - I break out of the loop when i find a "good enough" airport result - within a certain distance to my lookup coordinate (because there are only so many airports in a given area).

Another thing worth pointing out are the Collection objects used in the solution. Namely ConcurrentQueue type collections to hold ADS-B event and output flight information. This was designed with a multi threaded, "always on" future implemenaion in mind - since down the line this application would likely be run with different data input and output sources. More information is available in the "Future Improvements" section below.


# Running the Solution
The visual studio configuration file submitted is set to debug mode, so, to run and dive into the code, simply excecute it from Visual Studio(I used the latest Visual Studio Community Edition).
I have also included an excecutable built in "Release" configuration in the solution directory, located in the "run" folder. Open a command line processor in Windows, go to the "run" folder and type:
`ParagonCodingExercise.exe`
The output will be located in the "run\Resources" directory.
**Note** - I have not checked for needed dependencies, there might be some machines that would need additional .NET packages installed.



# Future Improvements

There are many potential improvements to this solution, some of them have alreaday been hinted at in the text above. They include but are not limited to:
* First and foremost - thoroughly analyze the data for all 49 aircraft in this data set to see if results are consistent and what improvements to the solution need to be made. The larger the confirmed working sample - the greater chance there will be fewer and fewer outliers.
* Potentially implement heading and altitude into the algorithm above, if needed.
* Creating detailed settings for the appication with detailed command line inputs.
* The ability to process only one airplane at a time.
* A separate service that checks if a particular flight time "makes sense". This can be populated from available flight data and stored locally. For example - a light from New York to Miami takes an average of 2 hours and 30 minutes. If our calculations don't reasonably match that (within, for example, a 45 minue window), we can conclude that our initial calculation was incorrect and we should revisit it and potentially change the ADS-B processing algorithm accordingly. 
* A Http based service that can look up and populate airplane data for more fine grained "real world" data.
* Rewrite the AirportCollections class to use Geographic Indexing and compare performance.
* More detailed build process.
* Implement a multithreaded approach to the solution so that data can be read, written and processed simultaneously.
* Implement the solution with different types of input data (http calls, etc).
* Clean up the code where needed. 
* Implement a thorough debugging and logging mechanism (I like log4net).
* Add abstraction to the included Unit Tests. There is currently too much copy/pasting within the unit test code.

