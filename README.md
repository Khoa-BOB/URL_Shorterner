# Design URL shortener
[ByteByteGo_Guidance](https://bytebytego.com/courses/system-design-interview/design-a-url-shortener)

## Problem:
- Write operation: 100 million URLs are generated per day
- Write operation per second: 100 milion / 24 / 3600 = 1160
- Read operation: Assuming ratio of read operation to write operation is 10:1, read operation per second: 1160 * 10 = 11600
- Assuming the URL shortener service will run for 10 years, this means we must support 100 million * 365 * 10 = 365 bilion records
- Assume average URL length is 100
- Storage requirement over 10 years: 365 bilion * 100 bytes - 36.5 TB

## API endpoints:

1. Url shortening: To create a new short URL, a client sends a POST request, which contains one parameter
2. Url redirecting: To redirect a short URL to the corresponding long URL, a lient send a GET request.

