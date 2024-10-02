# MeterApi

This is my interpetation of the ability to upload a csv of meter readings.

The API is a net core 8 with an endpoint is a POSt -> meter-reading-uploads and takes a file.

Initially I had attempted to use EF for the data store, however, I decvided on dapper which I felt gave more flexibility.

I have a series of controller tests that exercises the following:-

- The file being empty
- The file being null
- The file having invalid readings
- The file having valid readings

## Todo

- Better file structure
- Abstract the logic inside the controller to be in a service layer, this will give better seperation and add in Interface Segragation for better testing purposes
- Remove the need for a database within the solution and house one on a db server/dynamo etc

I hope you enjoy this and I apologise for the structure.