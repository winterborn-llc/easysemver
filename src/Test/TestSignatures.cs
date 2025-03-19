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
    
    private static Signature SignatureBase = new()
    {
        new SignatureClass
        {
            ClassName = "TestClass",
            Properties = new List<SignatureClassProperty>
            {
                new()
                {
                    Name = "TestProperty",
                    Type = "string",
                    IsReadable = true,
                    IsWritable = true
                }
            },
            Methods = new List<SignatureClassMethod>
            {
                new()
                {
                    MethodName = "TestMethod",
                    Parameters = new List<SignatureClassMethodInput>
                    {
                        new()
                        {
                            ParameterName = "input",
                            ParameterType = "string",
                            IsRequired = false
                        }
                    }
                }
            }
        }
    };
    
    private static Signature MajorSignatureOfReorderedParameters = new()
    {
        new SignatureClass
        {
            ClassName = "TestClass",
            Properties = new List<SignatureClassProperty>
            {
                new()
                {
                    Name = "TestProperty",
                    Type = "string",
                    IsReadable = true,
                    IsWritable = true
                }
            },
            Methods = new List<SignatureClassMethod>
            {
                new()
                {
                    MethodName = "TestMethod",
                    Parameters = new List<SignatureClassMethodInput>
                    {
                        new()
                        {
                            ParameterName = "newInput",
                            ParameterType = "string",
                            IsRequired = false
                        },
                        new()
                        {
                            ParameterName = "input",
                            ParameterType = "string",
                            IsRequired = false
                        }
                    }
                }
            }
        }
    };

    
    private static Signature MajorSignatureOfChangeOptionalParameterNowRequired = new()
    {
        new SignatureClass
        {
            ClassName = "TestClass",
            Properties = new List<SignatureClassProperty>
            {
                new()
                {
                    Name = "TestProperty",
                    Type = "string",
                    IsReadable = true,
                    IsWritable = true
                }
            },
            Methods = new List<SignatureClassMethod>
            {
                new()
                {
                    MethodName = "TestMethod",
                    Parameters = new List<SignatureClassMethodInput>
                    {
                        new()
                        {
                            ParameterName = "input",
                            ParameterType = "string",
                            IsRequired = true
                        }
                    }
                }
            }
        }
    };
    
    private static Signature MajorSignatureOfChangeMissingClass = new();
    
    private static Signature MajorSignatureOfChangeMissingMethod = new()
    {
        new SignatureClass
        {
            ClassName = "TestClass",
            Properties = new List<SignatureClassProperty>
            {
                new()
                {
                    Name = "TestProperty",
                    Type = "string",
                    IsReadable = true,
                    IsWritable = true
                }
            },
            Methods = new List<SignatureClassMethod>()
        }
    };
    
    private static Signature MajorSignatureOfChangedReturnType = new()
    {
        new SignatureClass
        {
            ClassName = "TestClass",
            Properties = new List<SignatureClassProperty>
            {
                new()
                {
                    Name = "TestProperty",
                    Type = "string",
                    IsReadable = true,
                    IsWritable = true
                }
            },
            Methods = new List<SignatureClassMethod>
            {
                new()
                {
                    MethodName = "TestMethod",
                    MethodType = "boolean",
                    Parameters = new List<SignatureClassMethodInput>
                    {
                        new()
                        {
                            ParameterName = "input",
                            ParameterType = "string",
                            IsRequired = false
                        }
                    }
                }
            }
        }
    };
    
    private static Signature MajorSignatureOfMethodMissingParameter = new()
    {
        new SignatureClass
        {
            ClassName = "TestClass",
            Properties = new List<SignatureClassProperty>
            {
                new()
                {
                    Name = "TestProperty",
                    Type = "string",
                    IsReadable = true,
                    IsWritable = true
                }
            },
            Methods = new List<SignatureClassMethod>
            {
                new()
                {
                    MethodName = "TestMethod",
                    MethodType = "boolean",
                    Parameters = new List<SignatureClassMethodInput>()
                }
            }
        }
    };

    private static Signature MajorSignatureOfMissingProperty = new()
    {
        new SignatureClass
        {
            ClassName = "TestClass",
            Properties = new List<SignatureClassProperty>(),
            Methods = new List<SignatureClassMethod>
            {
                new()
                {
                    MethodName = "TestMethod",
                    Parameters = new List<SignatureClassMethodInput>
                    {
                        new()
                        {
                            ParameterName = "input",
                            ParameterType = "string",
                            IsRequired = false
                        }
                    }
                }
            }
        }
    };

    private static Signature MajorSignatureOfChangePropertyType = new();
    
    private static Signature MajorSignatureOfChangedPropertyType = new()
    {
        new SignatureClass
        {
            ClassName = "TestClass",
            Properties = new List<SignatureClassProperty>
            {
                new()
                {
                    Name = "TestProperty",
                    Type = "boolean",
                    IsReadable = true,
                    IsWritable = true
                }
            },
            Methods = new List<SignatureClassMethod>
            {
                new()
                {
                    MethodName = "TestMethod",
                    MethodType = "boolean",
                    Parameters = new List<SignatureClassMethodInput>
                    {
                        new()
                        {
                            ParameterName = "input",
                            ParameterType = "string",
                            IsRequired = false
                        }
                    }
                }
            }
        }
    };
    
    private static Signature MajorSignatureOfChangedPropertyReadAccess = new()
    {
        new SignatureClass
        {
            ClassName = "TestClass",
            Properties = new List<SignatureClassProperty>
            {
                new()
                {
                    Name = "TestProperty",
                    Type = "string",
                    IsReadable = false,
                    IsWritable = true
                }
            },
            Methods = new List<SignatureClassMethod>
            {
                new()
                {
                    MethodName = "TestMethod",
                    MethodType = "boolean",
                    Parameters = new List<SignatureClassMethodInput>
                    {
                        new()
                        {
                            ParameterName = "input",
                            ParameterType = "string",
                            IsRequired = false
                        }
                    }
                }
            }
        }
    };
    
    private static Signature MajorSignatureOfChangedPropertyWriteAccess = new()
    {
        new SignatureClass
        {
            ClassName = "TestClass",
            Properties = new List<SignatureClassProperty>
            {
                new()
                {
                    Name = "TestProperty",
                    Type = "string",
                    IsReadable = true,
                    IsWritable = false
                }
            },
            Methods = new List<SignatureClassMethod>
            {
                new()
                {
                    MethodName = "TestMethod",
                    MethodType = "boolean",
                    Parameters = new List<SignatureClassMethodInput>
                    {
                        new()
                        {
                            ParameterName = "input",
                            ParameterType = "string",
                            IsRequired = false
                        }
                    }
                }
            }
        }
    };

    private static Signature MinorSignatureOfChangeNewClass = new()
    {
        new SignatureClass
        {
            ClassName = "TestClass",
            Properties = new List<SignatureClassProperty>
            {
                new()
                {
                    Name = "TestProperty",
                    Type = "string",
                    IsReadable = true,
                    IsWritable = true
                }
            },
            Methods = new List<SignatureClassMethod>
            {
                new()
                {
                    MethodName = "TestMethod",
                    Parameters = new List<SignatureClassMethodInput>
                    {
                        new()
                        {
                            ParameterName = "input",
                            ParameterType = "string",
                            IsRequired = false
                        }
                    }
                }
            }
        },
        new SignatureClass
        {
            ClassName = "NewClass",
            Methods = new List<SignatureClassMethod>()
        }
    };
    
    private static Signature MinorSignatureOfChangeNewProperty = new()
    {
        new SignatureClass
        {
            ClassName = "TestClass",
            Properties = new List<SignatureClassProperty>
            {
                new()
                {
                    Name = "TestProperty",
                    Type = "string",
                    IsReadable = true,
                    IsWritable = true
                },
                new()
                {
                    Name = "TestProperty2",
                    Type = "string",
                    IsReadable = true,
                    IsWritable = true
                }
            },
            Methods = new List<SignatureClassMethod>
            {
                new()
                {
                    MethodName = "TestMethod",
                    Parameters = new List<SignatureClassMethodInput>
                    {
                        new()
                        {
                            ParameterName = "input",
                            ParameterType = "string",
                            IsRequired = false
                        }
                    }
                }
            }
        }
    };
    
    private static Signature MinorSignatureOfChangeNewMethod = new()
    {
        new SignatureClass
        {
            ClassName = "TestClass",
            Properties = new List<SignatureClassProperty>
            {
                new()
                {
                    Name = "TestProperty",
                    Type = "string",
                    IsReadable = true,
                    IsWritable = true
                }
            },
            Methods = new List<SignatureClassMethod>
            {
                new()
                {
                    MethodName = "TestMethod",
                    Parameters = new List<SignatureClassMethodInput>
                    {
                        new()
                        {
                            ParameterName = "input",
                            ParameterType = "string",
                            IsRequired = false
                        }
                    }
                },
                new()
                {
                    MethodName = "TestMethod2"
                }
            }
        }
    };
    
    private static Signature MinorSignatureOfNewOptionalParameter = new()
    {
        new SignatureClass
        {
            ClassName = "TestClass",
            Properties = new List<SignatureClassProperty>
            {
                new()
                {
                    Name = "TestProperty",
                    Type = "string",
                    IsReadable = true,
                    IsWritable = true
                }
            },
            Methods = new List<SignatureClassMethod>
            {
                new()
                {
                    MethodName = "TestMethod",
                    Parameters = new List<SignatureClassMethodInput>
                    {
                        new()
                        {
                            ParameterName = "input",
                            ParameterType = "string",
                            IsRequired = false
                        },
                        new()
                        {
                            ParameterName = "optionalInput",
                            ParameterType = "string",
                            IsRequired = false
                        }
                    }
                }
            }
        }
    };
}