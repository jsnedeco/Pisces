import sys
import os
import subprocess
from glob import glob

RED   = "\033[1;31m"  
GREEN = "\033[0;32m"
RESET = "\033[0;0m"

src_root = os.path.abspath(os.path.join(os.getcwd(), os.pardir))
output_path = os.path.join(src_root,"scripts","bin","pisces_all")
exe_search_root = os.path.join(src_root,"exe","*", "*.csproj")
tools_search_root = os.path.join(src_root,"tools","*", "*.csproj")
print("building all projects to " + output_path)
#print("searchroot =" + search_root)


if not os.path.exists(output_path):
    os.makedirs(output_path)

exe_proj=glob(exe_search_root)
tools_proj=glob(tools_search_root)
proj_to_build=exe_proj+tools_proj
print(proj_to_build)

projectsbuilt=[]
for f in proj_to_build: 

    command =   (  "dotnet publish " + 
                "--output " +
                output_path + " " +
                "--self-contained " +
                "-r " + 
                "linux-x64 " +
                f + " " + 
                "/property:GenerateFullPaths=true " + 
                "/consoleloggerparameters:NoSummary " )

    print ("building " + f)
    print ("command: " +  command)
    fname = (os.path.basename(f))

    ret_val = subprocess.Popen(command, stdout=subprocess.PIPE, stderr=subprocess.PIPE, shell=True )
    output, errors = ret_val.communicate()

    sys.stdout.write(RED)
    print("BUILD ISSUE!!")
    print(errors)
    sys.stdout.write(RESET)

    print(output)

    if(len(errors)==0):
        projectsbuilt.append(fname)


#no exception handling is deliberate. let it all raise.

#no one wants this bloat, right.
debug_files = os.path.join(output_path, "*.pdb")
for f in glob(debug_files): 
    os.remove(f)

print("Projects built:")
print( projectsbuilt)
print('Building complete. Copy ' + output_path + ' to desired location to deploy.')  