﻿## Description
A sample job that implements IJobUI. A custom input field will be displayed in create schedule dialog. 

## Usage
Assuming BlazoriseQuartzApp is running under app/ folder
1. Build this example
2. Create a folder Jobs/Examples under app/
3. Copy the compiled binaries (*.dll) to app/Jobs/Examples
4. Open app/appsetting.json
5. Add "Jobs/Examples/BlazoriseQuartz.Examples.JobUI" under BlazoriseQuartz:AllowedJobAssemblyFiles. Ex.
```
  ...
  "BlazoriseQuartz": {
    ...
    "AllowedJobAssemblyFiles": [
      "Jobs/Examples/BlazoriseQuartz.Examples.JobUI"
    ]
  },
  ...
```