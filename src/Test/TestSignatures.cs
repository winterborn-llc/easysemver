using Yamamari.Library.AutoVersion;
using Yamamari.Library.AutoVersion.Signatures;

namespace Test;

public class TestSignatures
{
    [Fact]
    private void MajorIsRemovingType()
    {
        var oldSignature = SignatureBase;
        var newSignature = MajorSignatureOfChangeMissingClass;
        var changeLevel = SignatureComparer.GetChangeType(oldSignature, newSignature);
        Assert.Equal(VersionType.Major, changeLevel);
    }

    [Fact]
    private void MajorIsRemovingProperty()
    {
        var oldSignature = SignatureBase;
        var newSignature = MajorSignatureOfMissingProperty;
        var changeLevel = SignatureComparer.GetChangeType(oldSignature, newSignature);
        Assert.Equal(VersionType.Major, changeLevel);
    }

    [Fact]
    private void MajorIsRemovingPropertyRead()
    {
        var oldSignature = SignatureBase;
        var newSignature = MajorSignatureOfChangedPropertyReadAccess;
        var changeLevel = SignatureComparer.GetChangeType(oldSignature, newSignature);
        Assert.Equal(VersionType.Major, changeLevel);
    }

    [Fact]
    private void MajorIsRemovingPropertyWrite()
    {
        var oldSignature = SignatureBase;
        var newSignature = MajorSignatureOfChangedPropertyWriteAccess;
        var changeLevel = SignatureComparer.GetChangeType(oldSignature, newSignature);
        Assert.Equal(VersionType.Major, changeLevel);
    }

    [Fact]
    private void MajorIsRemovingMethod()
    {
        var oldSignature = SignatureBase;
        var newSignature = MajorSignatureOfChangeMissingMethod;
        var changeLevel = SignatureComparer.GetChangeType(oldSignature, newSignature);
        Assert.Equal(VersionType.Major, changeLevel);
    }

    [Fact]
    private void MajorIsChangedMethodType()
    {
        var oldSignature = SignatureBase;
        var newSignature = MajorSignatureOfChangedReturnType;
        var changeLevel = SignatureComparer.GetChangeType(oldSignature, newSignature);
        Assert.Equal(VersionType.Major, changeLevel);
    }

    [Fact]
    private void MajorIsMissingMethodParameter()
    {
        var oldSignature = SignatureBase;
        var newSignature = MajorSignatureOfMethodMissingParameter;
        var changeLevel = SignatureComparer.GetChangeType(oldSignature, newSignature);
        Assert.Equal(VersionType.Major, changeLevel);
    }

    [Fact]
    private void MajorIsReorderedParameters()
    {
        var oldSignature = SignatureBase;
        var newSignature = MajorSignatureOfReorderedParameters;
        var changeLevel = SignatureComparer.GetChangeType(oldSignature, newSignature);
        Assert.Equal(VersionType.Major, changeLevel);
    }

    [Fact]
    private void MajorIsOptionalParameterNowRequired()
    {
        var oldSignature = SignatureBase;
        var newSignature = MajorSignatureOfChangeOptionalParameterNowRequired;
        var changeLevel = SignatureComparer.GetChangeType(oldSignature, newSignature);
        Assert.Equal(VersionType.Major, changeLevel);
    }

    [Fact]
    private void MinorIsNewType()
    {
        var oldSignature = SignatureBase;
        var newSignature = MinorSignatureOfChangeNewClass;
        var changeLevel = SignatureComparer.GetChangeType(oldSignature, newSignature);
        Assert.Equal(VersionType.Minor, changeLevel);
    }

    [Fact]
    private void MinorIsNewProperty()
    {
        var oldSignature = SignatureBase;
        var newSignature = MinorSignatureOfChangeNewProperty;
        var changeLevel = SignatureComparer.GetChangeType(oldSignature, newSignature);
        Assert.Equal(VersionType.Minor, changeLevel);
    }

    [Fact]
    private void MinorIsNewMethod()
    {
        var oldSignature = SignatureBase;
        var newSignature = MinorSignatureOfChangeNewMethod;
        var changeLevel = SignatureComparer.GetChangeType(oldSignature, newSignature);
        Assert.Equal(VersionType.Minor, changeLevel);
    }

    [Fact]
    private void MinorIsNewOptionalMethodParameter()
    {
        var oldSignature = SignatureBase;
        var newSignature = MinorSignatureOfNewOptionalParameter;
        var changeLevel = SignatureComparer.GetChangeType(oldSignature, newSignature);
        Assert.Equal(VersionType.Minor, changeLevel);
    }
    
    private static readonly Signature SignatureBase =
    [
        new SignatureProject("TestProject")
        {
            new SignatureProjectClass
            {
                ClassName = "TestClass",
                Properties =
                [
                    new SignatureProjectClassProperty
                    {
                        Name = "TestProperty",
                        Type = "string",
                        IsReadable = true,
                        IsWritable = true
                    }
                ],
                Methods =
                [
                    new SignatureProjectClassMethod
                    {
                        MethodName = "TestMethod",
                        Parameters =
                        [
                            new SignatureProjectClassMethodInput
                            {
                                ParameterName = "input",
                                ParameterType = "string",
                                IsRequired = false
                            }
                        ]
                    }
                ]
            }
        }
    ];
    
    private static readonly Signature MajorSignatureOfReorderedParameters =
    [
        new SignatureProject("TestProject")
        {
            new SignatureProjectClass
            {
                ClassName = "TestClass",
                Properties =
                [
                    new SignatureProjectClassProperty
                    {
                        Name = "TestProperty",
                        Type = "string",
                        IsReadable = true,
                        IsWritable = true
                    }
                ],
                Methods =
                [
                    new SignatureProjectClassMethod
                    {
                        MethodName = "TestMethod",
                        Parameters =
                        [
                            new SignatureProjectClassMethodInput
                            {
                                ParameterName = "newInput",
                                ParameterType = "string",
                                IsRequired = false
                            },
                            new SignatureProjectClassMethodInput
                            {
                                ParameterName = "input",
                                ParameterType = "string",
                                IsRequired = false
                            }
                        ]
                    }
                ]
            }
        }
    ];

    
    private static readonly Signature MajorSignatureOfChangeOptionalParameterNowRequired =
    [
        new SignatureProject("TestProject")
        {
            new SignatureProjectClass
            {
                ClassName = "TestClass",
                Properties =
                [
                    new SignatureProjectClassProperty
                    {
                        Name = "TestProperty",
                        Type = "string",
                        IsReadable = true,
                        IsWritable = true
                    }
                ],
                Methods =
                [
                    new SignatureProjectClassMethod
                    {
                        MethodName = "TestMethod",
                        Parameters =
                        [
                            new SignatureProjectClassMethodInput
                            {
                                ParameterName = "input",
                                ParameterType = "string",
                                IsRequired = true
                            }
                        ]
                    }
                ]
            }
        }
    ];
    
    private static readonly Signature MajorSignatureOfChangeMissingClass = [];
    
    private static readonly Signature MajorSignatureOfChangeMissingMethod =
    [
        new SignatureProject("TestProject")
        {
            new SignatureProjectClass
            {
                ClassName = "TestClass",
                Properties =
                [
                    new SignatureProjectClassProperty
                    {
                        Name = "TestProperty",
                        Type = "string",
                        IsReadable = true,
                        IsWritable = true
                    }
                ],
                Methods = []
            }
        }
    ];
    
    private static readonly Signature MajorSignatureOfChangedReturnType =
    [
        new SignatureProject("TestProject")
        {
            new SignatureProjectClass
            {
                ClassName = "TestClass",
                Properties =
                [
                    new SignatureProjectClassProperty
                    {
                        Name = "TestProperty",
                        Type = "string",
                        IsReadable = true,
                        IsWritable = true
                    }
                ],
                Methods =
                [
                    new SignatureProjectClassMethod
                    {
                        MethodName = "TestMethod",
                        MethodType = "boolean",
                        Parameters =
                        [
                            new SignatureProjectClassMethodInput
                            {
                                ParameterName = "input",
                                ParameterType = "string",
                                IsRequired = false
                            }
                        ]
                    }
                ]
            }
        }
    ];
    
    private static readonly Signature MajorSignatureOfMethodMissingParameter =
    [
        new SignatureProject("TestProject")
        {
            new SignatureProjectClass
            {
                ClassName = "TestClass",
                Properties =
                [
                    new SignatureProjectClassProperty
                    {
                        Name = "TestProperty",
                        Type = "string",
                        IsReadable = true,
                        IsWritable = true
                    }
                ],
                Methods =
                [
                    new SignatureProjectClassMethod
                    {
                        MethodName = "TestMethod",
                        MethodType = "boolean",
                        Parameters = new List<SignatureProjectClassMethodInput>()
                    }
                ]
            }
        }
    ];

    private static readonly Signature MajorSignatureOfMissingProperty =
    [
        new SignatureProject("TestProject")
        {
            new SignatureProjectClass
            {
                ClassName = "TestClass",
                Properties = [],
                Methods =
                [
                    new SignatureProjectClassMethod
                    {
                        MethodName = "TestMethod",
                        Parameters =
                        [
                            new SignatureProjectClassMethodInput
                            {
                                ParameterName = "input",
                                ParameterType = "string",
                                IsRequired = false
                            }
                        ]
                    }
                ]
            }
        }
    ];

    private static readonly Signature MajorSignatureOfChangePropertyType = [];
    
    private static readonly Signature MajorSignatureOfChangedPropertyType =
    [
        new SignatureProject("TestProject")
        {
            new SignatureProjectClass
            {
                ClassName = "TestClass",
                Properties =
                [
                    new SignatureProjectClassProperty
                    {
                        Name = "TestProperty",
                        Type = "boolean",
                        IsReadable = true,
                        IsWritable = true
                    }
                ],
                Methods =
                [
                    new SignatureProjectClassMethod
                    {
                        MethodName = "TestMethod",
                        MethodType = "boolean",
                        Parameters =
                        [
                            new SignatureProjectClassMethodInput
                            {
                                ParameterName = "input",
                                ParameterType = "string",
                                IsRequired = false
                            }
                        ]
                    }
                ]
            }
        }
    ];
    
    private static readonly Signature MajorSignatureOfChangedPropertyReadAccess =
    [
        new SignatureProject("TestProject")
        {
            new SignatureProjectClass
            {
                ClassName = "TestClass",
                Properties =
                [
                    new SignatureProjectClassProperty
                    {
                        Name = "TestProperty",
                        Type = "string",
                        IsReadable = false,
                        IsWritable = true
                    }
                ],
                Methods =
                [
                    new SignatureProjectClassMethod
                    {
                        MethodName = "TestMethod",
                        MethodType = "boolean",
                        Parameters =
                        [
                            new SignatureProjectClassMethodInput
                            {
                                ParameterName = "input",
                                ParameterType = "string",
                                IsRequired = false
                            }
                        ]
                    }
                ]
            }

        }
    ];
    
    private static readonly Signature MajorSignatureOfChangedPropertyWriteAccess =
    [
        new SignatureProject("TestProject")
        {
            new SignatureProjectClass
            {
                ClassName = "TestClass",
                Properties =
                [
                    new SignatureProjectClassProperty
                    {
                        Name = "TestProperty",
                        Type = "string",
                        IsReadable = true,
                        IsWritable = false
                    }
                ],
                Methods =
                [
                    new SignatureProjectClassMethod
                    {
                        MethodName = "TestMethod",
                        MethodType = "boolean",
                        Parameters =
                        [
                            new SignatureProjectClassMethodInput
                            {
                                ParameterName = "input",
                                ParameterType = "string",
                                IsRequired = false
                            }
                        ]
                    }
                ]
            }
        }
    ];

    private static readonly Signature MinorSignatureOfChangeNewClass =
    [
        new SignatureProject("TestProject")
        {
            new SignatureProjectClass
            {
                ClassName = "TestClass",
                Properties =
                [
                    new SignatureProjectClassProperty
                    {
                        Name = "TestProperty",
                        Type = "string",
                        IsReadable = true,
                        IsWritable = true
                    }
                ],
                Methods =
                [
                    new SignatureProjectClassMethod
                    {
                        MethodName = "TestMethod",
                        Parameters =
                        [
                            new SignatureProjectClassMethodInput
                            {
                                ParameterName = "input",
                                ParameterType = "string",
                                IsRequired = false
                            }
                        ]
                    }
                ]
            },
            new SignatureProjectClass
            {
                ClassName = "NewClass",
                Methods = []
            }
        }
    ];
    
    private static readonly Signature MinorSignatureOfChangeNewProperty =
    [
        new SignatureProject("TestProject")
        {

            new SignatureProjectClass
            {
                ClassName = "TestClass",
                Properties =
                [
                    new SignatureProjectClassProperty
                    {
                        Name = "TestProperty",
                        Type = "string",
                        IsReadable = true,
                        IsWritable = true
                    },
                    new SignatureProjectClassProperty
                    {
                        Name = "TestProperty2",
                        Type = "string",
                        IsReadable = true,
                        IsWritable = true
                    }
                ],
                Methods =
                [
                    new SignatureProjectClassMethod
                    {
                        MethodName = "TestMethod",
                        Parameters =
                        [
                            new SignatureProjectClassMethodInput
                            {
                                ParameterName = "input",
                                ParameterType = "string",
                                IsRequired = false
                            }
                        ]
                    }
                ]
            }
        }
    ];
    
    private static readonly Signature MinorSignatureOfChangeNewMethod =
    [
        new SignatureProject("TestProject")
        {
            new SignatureProjectClass
            {
                ClassName = "TestClass",
                Properties =
                [
                    new SignatureProjectClassProperty
                    {
                        Name = "TestProperty",
                        Type = "string",
                        IsReadable = true,
                        IsWritable = true
                    }
                ],
                Methods =
                [
                    new SignatureProjectClassMethod
                    {
                        MethodName = "TestMethod",
                        Parameters =
                        [
                            new SignatureProjectClassMethodInput
                            {
                                ParameterName = "input",
                                ParameterType = "string",
                                IsRequired = false
                            }
                        ]
                    },

                    new SignatureProjectClassMethod
                    {
                        MethodName = "TestMethod2"
                    }
                ]
            }
        }
    ];
    
    private static readonly Signature MinorSignatureOfNewOptionalParameter =
    [
        new SignatureProject("TestProject")
        {
            new SignatureProjectClass
            {
                ClassName = "TestClass",
                Properties =
                [
                    new SignatureProjectClassProperty
                    {
                        Name = "TestProperty",
                        Type = "string",
                        IsReadable = true,
                        IsWritable = true
                    }
                ],
                Methods =
                [
                    new SignatureProjectClassMethod
                    {
                        MethodName = "TestMethod",
                        Parameters =
                        [
                            new SignatureProjectClassMethodInput
                            {
                                ParameterName = "input",
                                ParameterType = "string",
                                IsRequired = false
                            },
                            new SignatureProjectClassMethodInput
                            {
                                ParameterName = "optionalInput",
                                ParameterType = "string",
                                IsRequired = false
                            }
                        ]
                    }
                ]
            }
        }
    ];
}