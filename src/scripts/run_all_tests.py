import sys
import os
import subprocess
from glob import glob

RED   = "\033[1;31m"  
GREEN = "\033[0;32m"
RESET = "\033[0;0m"

src_root = os.path.abspath(os.path.join(os.getcwd(), os.pardir))
search_root = os.path.join(src_root,"test","*", "*.csproj")
print("running all tests....")
print("searchroot =" + search_root)

alltestouput=[]
totaltests={}
passedtests={}
failedtests={}
testingtime={}
numTotalTests=0.0;
numPassingTests=0.0;
projectsinspected=0

for f in glob(search_root): 

  #its 5 o'clock somewhere..
  #if projectsinspected > 6:
  #	break


  print ("testing " + f)
  os.chdir(os.path.dirname(f))
  fname = (os.path.basename(f))

  try:
   testouput = subprocess.check_output(["dotnet", "test"]).split('\n')
  except:

   sys.stdout.write(RED)
   print("Test threw excection in " + f)
   print(testouput)
   print("Unexpected error:", sys.exc_info()[0])
   sys.stdout.write(RESET)
   failedtests[f]=testouput
   numTotalTests+=numTests;
   numPassingTests+=0;
   #break
   pass


  alltestouput.append(testouput)
  testingtime[fname]=testouput[len(testouput)-2].replace(" Total time: ","").strip()
  
  numTests=float(testouput[len(testouput)-4].replace("Total tests: ","").strip())
  numPass=float(testouput[len(testouput)-3].replace("Passed: ","").strip())
  numFail=numTests-numPass

  totaltests[fname]=numTests
  passedtests[fname]=numPass
  numTotalTests+=numTests;
  numPassingTests+=numPass;

  projectsinspected+=1;

  if numTests < 3:
      print(fname + " needs more tests")
      continue
 
  percentpassing=100.0*numPass/numTests
  if percentpassing < 100:
      sys.stdout.write(RED)
      print("precent passing: " + str(100.0*numPass/numTests))
      sys.stdout.write(RESET)
  else:
        sys.stdout.write(GREEN)
  	print("precent passing: " + str(100.0*numPass/numTests))
	sys.stdout.write(RESET)

print("-----------------------------------------------------------------------------------------------")
print("testing time:")
print(testingtime)

print("tests ran:")
print(totaltests)

print("tests passed:")
print(passedtests)

print("failures in:")
print(failedtests.keys())


if numTotalTests < 3:
        sys.stdout.write(RED)
	print("I will eat my hat")
	print("Num tests ran:" + str(numTests))
else:
	print("percent passing: " + str(100.0*numPassingTests/numTotalTests))
	print("total num tests: " + str(numTotalTests))

