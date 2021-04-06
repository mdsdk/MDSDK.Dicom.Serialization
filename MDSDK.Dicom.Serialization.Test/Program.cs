// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Xml.Linq;

namespace MDSDK.Dicom.Serialization.Examples
{
    class Program
    {
        static readonly string SourceDicomDataUrlPrefix = "ftp://medical.nema.org/MEDICAL/Dicom/DataSets";

        static readonly string CachedDicomDataFolder = Path.Combine(Path.GetTempPath(), "MDSDK.Dicom.Serialization.Test");

        static FileStream OpenRead(string relativePath)
        {
            var cachedFilePath = Path.Combine(CachedDicomDataFolder, relativePath);
            if (!File.Exists(cachedFilePath))
            {
                var outputFolder = Path.GetDirectoryName(cachedFilePath);
                Directory.CreateDirectory(outputFolder);

                var sourceUrl = $"{SourceDicomDataUrlPrefix}/{relativePath}";
                Console.WriteLine($"Downloading {sourceUrl}");

                var request = WebRequest.Create(sourceUrl);
                request.Method = WebRequestMethods.Ftp.DownloadFile;
                using var response = request.GetResponse();
                using var input = response.GetResponseStream();
                using var output = File.OpenWrite(cachedFilePath);
                input.CopyTo(output);
            }

            Console.WriteLine($"Reading {cachedFilePath}");
            return File.OpenRead(cachedFilePath);
        }

        static void Test(string relativePath)
        {
            try
            {
                using (var stream = OpenRead(relativePath))
                {
                    if (DicomFileReader.TryCreate(stream, out DicomFileReader dicomFileReader))
                    {
                        Console.WriteLine($"SOPClassUID = {dicomFileReader.MediaStorageSOPClassUID}");
                        Console.WriteLine($"TransferSyntax = {dicomFileReader.TransferSyntax}");

                        Console.WriteLine(JsonSerializer.Serialize(dicomFileReader.FileMetaInformation, new JsonSerializerOptions
                        {
                            WriteIndented = true
                        }));

                        var xmlOutputPath = $"{stream.Name}.xml";
                        Console.WriteLine($"Writing dataset to {xmlOutputPath}");

                        var dataSet = new XElement("DicomDataSet");
                        dataSet.SetAttributeValue("Source", stream.Name);
                        dicomFileReader.DataSetReader.ReadDataSet(dataSet, new DicomToXmlConverter());
                        dataSet.Save(xmlOutputPath);
                    }
                    else
                    {
                        Console.WriteLine($"Skipped");
                    }
                }
            }
            catch (Exception error)
            {
                Console.WriteLine($"ERROR: {error.Message}");
            }
        }

        static readonly string[] TestFiles = new[]
        {
            "WG02/Enhanced-XA/GENECG",
            "WG16/Philips/ClassicSingleFrame/Brain/DICOM/IM_0001",
            "WG16/Philips/ClassicSingleFrame/Brain/DICOM/PS_0023",
            "WG16/Philips/EnhancedMR/Brain/DICOM/IM_0001",
            "WG16/Siemens/Siemens_spectroEnhDICOM/Anonymous.MR._.23.1.2017.06.28.09.41.02.294.59320560.dcm"
        };

        static void Main()
        {
            foreach (var testFile in TestFiles)
            {
                Test(testFile);
            }
        }
    }
}
