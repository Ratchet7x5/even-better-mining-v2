using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using UAssetAPI;
using UAssetAPI.ExportTypes;
using UAssetAPI.CustomVersions;
using UAssetAPI.FieldTypes;
using UAssetAPI.IO;
using UAssetAPI.JSON;
using UAssetAPI.Kismet;
using UAssetAPI.PropertyTypes.Objects;
using UAssetAPI.PropertyTypes.Structs;
using UAssetAPI.UnrealTypes;


namespace OHML_Mod_DRG
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] fileNames = { };
            var hitsNotFound = new List<string>();
            var pickAxeNotFound = new List<string>();

            try
            {
                // Open the text file using a stream reader.
                using (StreamReader sr = new StreamReader("TextureFileNames.txt"))
                {
                    // read txt file
                    String currentFileName = (sr.ReadToEnd());
                    char[] sep = { '\r', '\n' }; // delimiters

                    // parse file names
                    fileNames = currentFileName.Split(sep, StringSplitOptions.RemoveEmptyEntries); //remove empty "" strings as well
                }
            }
            catch (IOException e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
            }

            Console.WriteLine("Now opening the following files to edit:");

            IntPropertyData PD_HitsNeededToMine = null;
            FloatPropertyData PD_PickAxeDigSize = null;

            //store the property data for inspection later
            var hitsList = new List<PropertyData>();
            var pickList = new List<PropertyData>();

            foreach (String fileName in fileNames)
            {
                int numOfFiles = fileNames.Length;
                String folderPath = ".\\Materials\\";
                String fileToEdit = folderPath + fileName;

                // All files must be inside the bin\Debug\Materials folder. 
                UAsset myAsset = new UAsset(fileToEdit, EngineVersion.VER_UE4_27);

                // In export[0], we need to find TerrainMaterial, and then find the properties "HitsNeededToMine" (1) and "PickAxeDigSize" (205).
                // There are many types, but any export that has regular "tagged" data like you see as properties in UAssetGUI can be cast to a NormalExport, like this one.
                NormalExport myExport = (NormalExport)myAsset.Exports[0];
                Console.WriteLine(myExport.ObjectName);

                //if the file is "TM_BEDROCK", then save [4] HitsNeededToMine and [5] PickAxeDigSize entries
                // we'll try to copy the HitNeededToMine (IntPropertyData) and PickAxeDigSize (FloatPropertyData) into 
                // the Data (List<>) found in myExport
                if (myExport.ObjectName.ToString().Equals("TM_BedRock") == true)
                {
                    PD_HitsNeededToMine = (IntPropertyData)myExport.Data[4];
                    PD_PickAxeDigSize = (FloatPropertyData)myExport.Data[5];
                }
                
                // if PickAxeDigSize doesn't exist, log it
                if (myExport["PickAxeDigSize"] == null)
                {
                    Console.WriteLine("PickAxeDigSize: not found");
                    pickAxeNotFound.Add(fileName);
                    //FString name = new FString("PickAxeDigSize");
                    //myExport.Asset.AddNameReference(name, false);
                    //myExport.Data.Add(PD_PickAxeDigSize);
                }
                else // if it does, edit the value
                {
                    FloatPropertyData PickAxeDigSize = (FloatPropertyData)myExport["PickAxeDigSize"];
                    PickAxeDigSize.Value = 205f;
                    pickList.Add(myExport["PickAxeDigSize"]);
                    Console.WriteLine("PickAxeDigSize: " + PickAxeDigSize.Value);
                }

                // if HitsNeededToMine doesn't exist, log it
                if (myExport["HitsNeededToMine"] == null)
                {
                    Console.WriteLine("HitsNeededToMine: not found");
                    hitsNotFound.Add(fileName);
                }
                else // if it does, edit the value
                {
                    IntPropertyData HitsNeededToMine = (IntPropertyData)myExport["HitsNeededToMine"];
                    HitsNeededToMine.Value = 1;
                    hitsList.Add(myExport["HitsNeededToMine"]);
                    Console.WriteLine("HitsNeededToMine: " + HitsNeededToMine.Value);
                }

                // Save the asset back to disk with UAsset.Write and a path.
                // path = same path the asset was taken from. 
                Console.WriteLine("Saving " + myExport.ObjectName + " to bin\\Debug...\n");

                //assigns name and then exports to the path specified
                myAsset.Write(fileToEdit);
            }

            if (hitsNotFound.Any())
            {
                Console.WriteLine("Files where HitsNeededToMine were not found: \n" + string.Join("\n", hitsNotFound) + "\n");
            }

            if (pickAxeNotFound.Any())
            {
                Console.WriteLine("Files where PickAxeDigSize were not found: \n" + string.Join("\n", pickAxeNotFound) + "\n");
            }

            Console.WriteLine("#Files with HitsNeededToMine: " + hitsList.Count);
            Console.WriteLine("#Files without HitsNeededToMine: " + hitsNotFound.Count);
            Console.WriteLine("#Files with PickAxeDigSize: " + pickList.Count);
            Console.WriteLine("#Files without PickAxeDigSize: " + pickAxeNotFound.Count);
            Console.WriteLine("Press Enter twice to exit the program...");
            Console.ReadLine();
        }
    }
}