# Okolariet

## Update refitter file after openapi.yaml changes
Note that to run a fresh refitter so that the files are updated after new changes are made to the openapi.yaml file, do the following:
1. Deleting the old file is not necessary, but doesn't remove the possibility
2. Restart the PRISM container, first stop the old Docker Container by doing this:
- docker ps
- docker stop <container id or name from the list> 
3. Now go to the filepath for the project and open a CMD: ...\source\repos\Okolariet-Exam.
4. Run this: 
- docker run --rm -v "%cd%\contracts":/tmp -p 4010:4010 stoplight/prism:5 mock -h 0.0.0.0 --multiprocess=false /tmp/openapi.yaml
5. Make sure not to close the window, the PRISM output is easiest to see in the console itself
6. Open a new CMD on the Okolariet-Exam, likely: ....\repos\Okolariet-Exam
7. Run this in in the CMD (Only necessary once per session): 
- dotnet tool restore 
8. Run the following:
- dotnet refitter --settings-file src\web\.refitter
