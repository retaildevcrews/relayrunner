# API Parameter Validation

The following documentation describes the types of Query String and route parameters available in the REST API. The categories are broken down by the [Load Clients API](##Load-Clients-API).

## Overview

The following section describes the following interactions with the API; 'direct reads'. A direct read involves specifying a `LoadClientId` in the route path without Query String parameters.

### Direct Reads

- Valid single read returns `HTTP/200` with `Movie` or `Actor` and content-type of `application/json`
- Valid single read with no results returns `HTTP/404` and content-type of `application/json`
- Invalid single read returns a `HTTP/400` error response with a `application/problem+json` content type

### Error Handling

The error handling details including the response to parameter or route path validation errors uses a combination of RFC 7807 and the Microsoft REST API guidelines and can be found on the [HttpErrorResponses](HttpErrorResponses.md) page.

## Load Clients API

### Load Clients Direct Read

This applies to the following API route:

- /api/loadClients/{loadClientId}

|   Name          |  Description                                       |  Type    |  Valid Input                     |  Response Body          |  Notes  |
|   ----          |  -----------                                       |  ----    |  -----------                     |  -------------          |  -----  |
|   loadClientId  |  Return a specific load client from the results    |  string  |  numeric integer greater than -1 |  Single `LoadClient`    |         |
