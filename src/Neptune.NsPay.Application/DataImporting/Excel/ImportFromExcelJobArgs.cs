﻿using System;
using Abp;

namespace Neptune.NsPay.DataImporting.Excel;

public class ImportFromExcelJobArgs
{
    public int? TenantId { get; set; }

    public Guid BinaryObjectId { get; set; }

    public UserIdentifier ExcelImporter { get; set; }
}