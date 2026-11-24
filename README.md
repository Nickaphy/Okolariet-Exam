# Okolariet

## Starting the project
To start the project, you must first Configure the start up project.
The system requires 2, things to be configured here.
Set both the Web and Host to start. The web handles all of the UI (WASM), the Host serves as a catch all for the server. It ensures that the API, Domain (Application) and Database is running

To start the project a user must also handle JWT authentication

Setting up a password for the docker file.

## Update refitter file after openapi.yaml changes (Run PRISM See step 3, 4 and 5)
Note that to run a fresh refitter so that the files are updated after new changes are made to the openapi.yaml file, do the following:
1. Deleting the old file is not necessary, but doesn't remove the possibility
2. Restart the PRISM container, first stop the old Docker Container by doing this (NOTE: Closing the Console will often cause PRISM to trash the container):
- docker ps
- docker stop <container id or name from the list> 
3. Now go to the filepath for the project and open a CMD: ...\source\repos\Okolariet-Exam.
4. Run this: 
- docker run --rm -v "%cd%\contracts":/tmp -p 4010:4010 stoplight/prism:5 mock -h 0.0.0.0 --multiprocess=false /tmp/openapi.yaml
5. Make sure not to close the window!!!! PRISM closes it's container by default if you do so
 - Note: Aspire and the lack of backend means you can't just run the program and use PRISM. You need to ONLY run .Web. Do this in a CMD
            - dotnet run --project src/Web --launch-profile http
            - Go to http://localhost:5132
6. Open a new CMD on the Okolariet-Exam, likely: ....\repos\Okolariet-Exam
7. Run this in in the CMD (Only necessary once per session): 
- dotnet tool restore 
8. Run the following:
- dotnet refitter --settings-file src\web\.refitter
