// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using MDSDK.Dicom.Serialization.TransferSyntaxes;
using System;
using System.Collections.Generic;

namespace MDSDK.Dicom.Serialization
{
    public static class DicomTransferSyntax
    {
        public static readonly ImplicitVRLittleEndian ImplicitVRLittleEndian = new();
        public static readonly ExplicitVRLittleEndian ExplicitVRLittleEndian = new();

        private static readonly Dictionary<string, TransferSyntax> s_tsMap;

        static DicomTransferSyntax()
        {
            s_tsMap = new Dictionary<string, TransferSyntax>();

            foreach (var field in typeof(DicomTransferSyntax).GetFields())
            {
                if (typeof(TransferSyntax).IsAssignableFrom(field.FieldType))
                {
                    var transferSyntax = (TransferSyntax)field.GetValue(null);
                    s_tsMap.Add(transferSyntax.DicomUID.UID, transferSyntax);
                }
            }
        }

        public static bool TryLookup(string uid, out TransferSyntax transferSyntax)
        {
            return s_tsMap.TryGetValue(uid, out transferSyntax);
        }
    }
}
