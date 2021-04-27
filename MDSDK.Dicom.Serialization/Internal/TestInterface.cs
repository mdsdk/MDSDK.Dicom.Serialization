// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

#pragma warning disable CS1591

using MDSDK.Dicom.Serialization.ValueRepresentations;

namespace MDSDK.Dicom.Serialization.Internal
{
    public static class TestInterface
    {
        public static string DecodeString(string[] specificCharacterSets, string vrs, byte[] bytes)
        {
            var dicomStringDecoder = StringDecoder.Get(specificCharacterSets);
            var vr = DicomVR.Lookup(vrs);
            return dicomStringDecoder.Decode(vr, bytes);
        }
    }
}

#pragma warning restore    
