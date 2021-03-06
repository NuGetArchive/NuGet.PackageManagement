﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace NuGet.ProjectManagement
{
    public class Token
    {
        public string Value { get; private set; }
        public TokenCategory Category { get; private set; }

        public Token(TokenCategory category, string value)
        {
            Category = category;
            Value = value;
        }
    }

    public enum TokenCategory
    {
        Text,
        Variable
    }
}
