# Training Exercise Automation CoinMarketCap

## Description

For this exercise we will continue using the CoinMarketCap connector created in the HTTP training.

The goal of this exercise is to create an automation script that will retrieve data from the cryptocurrencies tables on the CoinMarketCap elements and export them to a CSV file.

To be able to make this exercise in line with a real-life use-case we'll create three CoinMarketCap elements so we can query multiple tables.

The script should contain a single input parameter with the name of the folder in which the export files will be stored. The script itself should search all CoinMarketCap elements on the DMA, retrieve the data from those elements and export the currencies from every element to a separate CSV file. The name of the file should be [ElementName].csv. These files should be stored in the C:\Skyline DataMiner\Documents\[FolderName from inputparameter] folder.

Optional:

Use the scheduler module to execute this script every hour on your local DMA.

## Pointers

* Use the IDms class available from the Skyline.DataMiner.Core.DataMinerSystem.Automation Nuget\
  Package (entrypoint: Engine.GetDms()).
* Use logging (either IEngine.GenerateInformation or IEngine.Log to debug your scripts).
