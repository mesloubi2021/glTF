﻿using System;
using System.CodeDom;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using glTFLoader.Shared;
using Newtonsoft.Json.Linq;

namespace GeneratorLib
{
    public class SingleValueCodegenTypeFactory
    {
        public static CodegenType MakeCodegenType(string name, Schema schema)
        {
            CodegenType returnType = new CodegenType();
            EnforceRestrictionsOnSetValues(returnType, name, schema);

            if (schema.Format == "uri")
            {
                returnType.Attributes = new CodeAttributeDeclarationCollection
                {
                    new CodeAttributeDeclaration(
                        "Newtonsoft.Json.JsonConverterAttribute",
                        new[]
                        {
                            new CodeAttributeArgument(new CodeTypeOfExpression(typeof (UriConverter))),
                            new CodeAttributeArgument(
                                new CodeArrayCreateExpression(typeof (object), new CodeExpression[]
                                {
                                    new CodePrimitiveExpression(schema.Required)
                                })
                                )
                        }
                        )
                };
                switch (schema.UriType)
                {
                    case UriType.Application:
                        returnType.CodeType = new CodeTypeReference(typeof(byte[]));
                        break;
                    case UriType.Text:
                        returnType.CodeType = new CodeTypeReference(typeof(string));
                        break;
                    case UriType.Image:
                        returnType.CodeType = new CodeTypeReference(typeof(Bitmap));
                        break;
                    case UriType.None:
                        throw new InvalidDataException("UriType must be specified in the schema");
                }

                return returnType;
            }

            if (schema.Type.Length > 1)
            {
                returnType.CodeType = new CodeTypeReference(typeof(object));
                returnType.AdditionalMembers.Add(CreateMethodThatChecksIfTheValueOfAMemberIsNotEqualToAnotherExpression(name, new CodePrimitiveExpression(null)));
                return returnType;
            }

            var typeRef = schema.Type[0];
            if (typeRef.IsReference)
            {
                throw new NotImplementedException();
            }

            if (typeRef.Name == "any")
            {
                if (schema.Enum != null || schema.Default != null)
                {
                    throw new NotImplementedException();
                }

                returnType.CodeType = new CodeTypeReference(typeof(object));
                returnType.AdditionalMembers.Add(CreateMethodThatChecksIfTheValueOfAMemberIsNotEqualToAnotherExpression(name, new CodePrimitiveExpression(null)));
                return returnType;
            }

            if (typeRef.Name == "object")
            {
                if (schema.Enum != null || schema.HasDefaultValue())
                {
                    throw new NotImplementedException();
                }

                if (schema.Title != null)
                {
                    returnType.CodeType = new CodeTypeReference(Helpers.ParseTitle(schema.Title));
                    returnType.AdditionalMembers.Add(CreateMethodThatChecksIfTheValueOfAMemberIsNotEqualToAnotherExpression(name, new CodePrimitiveExpression(null)));
                    return returnType;
                }
                throw new NotImplementedException();
            }

            if (typeRef.Name == "number")
            {
                if (schema.Enum != null)
                {
                    throw new NotImplementedException();
                }

                if (schema.HasDefaultValue())
                {
                    returnType.DefaultValue = new CodePrimitiveExpression((float)(double)schema.Default);
                    returnType.AdditionalMembers.Add(CreateMethodThatChecksIfTheValueOfAMemberIsNotEqualToAnotherExpression(name, returnType.DefaultValue));
                }
                returnType.CodeType = new CodeTypeReference(typeof(float));
                return returnType;
            }

            if (typeRef.Name == "string")
            {
                if (schema.Enum != null)
                {
                    var enumType = GenStringEnumType(name, schema);
                    returnType.AdditionalMembers.Add(enumType);
                    returnType.CodeType = new CodeTypeReference(enumType.Name);

                    if (schema.HasDefaultValue())
                    {
                        for (var i = 0; i < enumType.Members.Count; i++)
                        {
                            if (enumType.Members[i].Name == schema.Default.ToString())
                            {
                                returnType.DefaultValue = new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(enumType.Name), (string)schema.Default);
                                returnType.AdditionalMembers.Add(CreateMethodThatChecksIfTheValueOfAMemberIsNotEqualToAnotherExpression(name, returnType.DefaultValue));
                                return returnType;
                            }
                        }
                        throw new InvalidDataException("The default value is not in the enum list");
                    }
                    returnType.AdditionalMembers.Add(CreateMethodThatChecksIfTheValueOfAMemberIsNotEqualToAnotherExpression(name, new CodePrimitiveExpression(null)));
                    return returnType;
                }

                if (schema.HasDefaultValue())
                {
                    returnType.DefaultValue = new CodePrimitiveExpression((string)schema.Default);
                    returnType.AdditionalMembers.Add(CreateMethodThatChecksIfTheValueOfAMemberIsNotEqualToAnotherExpression(name, returnType.DefaultValue));
                }
                returnType.CodeType = new CodeTypeReference(typeof(string));
                return returnType;
            }

            if (typeRef.Name == "integer")
            {
                if (schema.Enum != null)
                {
                    var enumType = GenIntEnumType(name, schema);
                    returnType.AdditionalMembers.Add(enumType);
                    returnType.CodeType = new CodeTypeReference(enumType.Name);

                    if (schema.HasDefaultValue())
                    {
                        returnType.DefaultValue = GetEnumField(enumType, (int)(long)schema.Default);
                        returnType.AdditionalMembers.Add(CreateMethodThatChecksIfTheValueOfAMemberIsNotEqualToAnotherExpression(name, returnType.DefaultValue));
                    }

                    return returnType;
                }

                if (schema.Default != null)
                {
                    returnType.DefaultValue = new CodePrimitiveExpression((int)(long)schema.Default);
                    returnType.AdditionalMembers.Add(CreateMethodThatChecksIfTheValueOfAMemberIsNotEqualToAnotherExpression(name, returnType.DefaultValue));
                }

                returnType.CodeType = new CodeTypeReference(typeof(int));
                return returnType;
            }

            if (typeRef.Name == "boolean")
            {
                if (schema.Enum != null)
                {
                    throw new NotImplementedException();
                }

                if (schema.Default != null)
                {
                    returnType.DefaultValue = new CodePrimitiveExpression((bool)schema.Default);
                    returnType.AdditionalMembers.Add(CreateMethodThatChecksIfTheValueOfAMemberIsNotEqualToAnotherExpression(name, returnType.DefaultValue));
                }
                returnType.CodeType = new CodeTypeReference(typeof(bool));
                return returnType;
            }

            throw new NotImplementedException(typeRef.Name);
        }

        private static void EnforceRestrictionsOnSetValues(CodegenType returnType, string name, Schema schema)
        {
            if (schema.Minimum != null)
            {
                returnType.SetStatements.Add(new CodeConditionStatement
                {
                    Condition = new CodeBinaryOperatorExpression
                    {
                        Left = new CodePropertySetValueReferenceExpression(),
                        Operator = schema.ExclusiveMinimum ? CodeBinaryOperatorType.LessThanOrEqual : CodeBinaryOperatorType.LessThan,
                        Right = new CodePrimitiveExpression(schema.Minimum)
                    },
                    TrueStatements =
                    {
                        new CodeThrowExceptionStatement
                        {
                            ToThrow = new CodeObjectCreateExpression
                            {
                                CreateType = new CodeTypeReference(typeof(ArgumentOutOfRangeException)),
                                Parameters =
                                {
                                    new CodePrimitiveExpression(name),
                                    new CodePropertySetValueReferenceExpression(),
                                    new CodePrimitiveExpression(
                                        schema.ExclusiveMinimum ?
                                            $"Expected value to be less than or equal to {schema.Minimum}" :
                                            $"Expected value to be less than {schema.Minimum}")
                                }
                            }
                        }
                    }
                });
            }
            if (schema.Maximum != null)
            {
                returnType.SetStatements.Add(new CodeConditionStatement
                {
                    Condition = new CodeBinaryOperatorExpression
                    {
                        Left = new CodePropertySetValueReferenceExpression(),
                        Operator = schema.ExclusiveMaximum ? CodeBinaryOperatorType.GreaterThanOrEqual : CodeBinaryOperatorType.GreaterThan,
                        Right = new CodePrimitiveExpression(schema.Maximum)
                    },
                    TrueStatements =
                    {
                        new CodeThrowExceptionStatement
                        {
                            ToThrow = new CodeObjectCreateExpression
                            {
                                CreateType = new CodeTypeReference(typeof(ArgumentOutOfRangeException)),
                                Parameters =
                                {
                                    new CodePrimitiveExpression(name),
                                    new CodePropertySetValueReferenceExpression(),
                                    new CodePrimitiveExpression(
                                        schema.ExclusiveMaximum ?
                                        $"Expected value to be greater than or equal to {schema.Maximum}" :
                                        $"Expected value to be greater than {schema.Maximum}")
                                }
                            }
                        }
                    }
                });
            }
            if (schema.MinLength != null)
            {
                returnType.SetStatements.Add(new CodeConditionStatement
                {
                    Condition = new CodeBinaryOperatorExpression
                    {
                        Left = new CodePropertyReferenceExpression
                        {
                            PropertyName = "Length",
                            TargetObject = new CodePropertySetValueReferenceExpression()
                        },
                        Operator = CodeBinaryOperatorType.LessThan,
                        Right = new CodePrimitiveExpression(schema.MinLength)
                    },
                    TrueStatements =
                    {
                        new CodeThrowExceptionStatement
                        {
                            ToThrow = new CodeObjectCreateExpression
                            {
                                CreateType = new CodeTypeReference(typeof(ArgumentException)),
                                Parameters =
                                {
                                    new CodePrimitiveExpression($"Expected the string length to be greater than or equal to {schema.MinLength}"),
                                    new CodePrimitiveExpression(name),
                                }
                            }
                        }
                    }
                });
            }
            if (schema.MaxLength != null)
            {
                returnType.SetStatements.Add(new CodeConditionStatement
                {
                    Condition = new CodeBinaryOperatorExpression
                    {
                        Left = new CodePropertyReferenceExpression
                        {
                            PropertyName = "Length",
                            TargetObject = new CodePropertySetValueReferenceExpression()
                        },
                        Operator = CodeBinaryOperatorType.GreaterThan,
                        Right = new CodePrimitiveExpression(schema.MaxLength)
                    },
                    TrueStatements =
                    {
                        new CodeThrowExceptionStatement
                        {
                            ToThrow = new CodeObjectCreateExpression
                            {
                                CreateType = new CodeTypeReference(typeof(ArgumentException)),
                                Parameters =
                                {
                                    new CodePrimitiveExpression($"Expected the string length to be less than or equal to {schema.MaxLength}"),
                                    new CodePrimitiveExpression(name),
                                }
                            }
                        }
                    }
                });
            }
        }

        public static CodeTypeDeclaration GenStringEnumType(string name, Schema schema)
        {
            var enumName = $"{name}Enum";
            var enumType = new CodeTypeDeclaration()
            {
                IsEnum = true,
                Attributes = MemberAttributes.Public,
                Name = enumName
            };

            foreach (var value in (JArray)schema.Enum)
            {
                enumType.Members.Add(new CodeMemberField(enumName, (string)value));
            }

            return enumType;
        }
        public static CodeTypeDeclaration GenIntEnumType(string name, Schema schema)
        {
            var enumName = $"{name}Enum";
            var enumType = new CodeTypeDeclaration()
            {
                IsEnum = true,
                Attributes = MemberAttributes.Public,
                Name = enumName
            };

            if (schema.EnumNames == null || ((JArray)schema.Enum).Count != schema.EnumNames.Length)
            {
                throw new InvalidOperationException("Enum names must be defined for each integer enum");
            }

            foreach (var index in Enumerable.Range(0, schema.EnumNames.Length))
            {
                var value = (int)(long)((JArray)schema.Enum)[index];
                enumType.Members.Add(new CodeMemberField(enumName, schema.EnumNames[index])
                {
                    InitExpression = new CodePrimitiveExpression(value)
                });
            }

            return enumType;
        }

        public static CodeFieldReferenceExpression GetEnumField(CodeTypeDeclaration enumType, int value)
        {
            var defaultMember = enumType.Members.Cast<CodeMemberField>().FirstOrDefault(m => (int)((CodePrimitiveExpression)m.InitExpression).Value == value);

            if (defaultMember == null)
            {
                throw new InvalidDataException("The default value is not in the enum list");
            }

            return new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(enumType.Name), defaultMember.Name);
        }

        public static CodeMemberMethod CreateMethodThatChecksIfTheValueOfAMemberIsNotEqualToAnotherExpression(
            string name, CodeExpression expression)
        {
            return new CodeMemberMethod
            {
                ReturnType = new CodeTypeReference(typeof(bool)),
                Statements =
                {
                    new CodeMethodReturnStatement()
                    {
                        Expression = new CodeBinaryOperatorExpression()
                        {
                            Left = new CodeBinaryOperatorExpression()
                            {
                                Left = new CodeFieldReferenceExpression()
                                {
                                    FieldName = "m_" + name.Substring(0, 1).ToLower() + name.Substring(1)
                                },
                            Operator = CodeBinaryOperatorType.ValueEquality,
                            Right = expression
                            },
                            Operator = CodeBinaryOperatorType.ValueEquality,
                            Right = new CodePrimitiveExpression(false)
                        }
                    }
                },
                Attributes = MemberAttributes.Public | MemberAttributes.Final,
                Name = "ShouldSerialize" + name
            };
        }
    }
}
