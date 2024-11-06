using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace Hasmer {
    public class ObjectDumper {
        private int Level;
        private readonly int IndentSize;
        private readonly StringBuilder Builder;
        private readonly List<int> FoundElements;

        private ObjectDumper(int indentSize) {
            IndentSize = indentSize;
            Builder = new StringBuilder();
            FoundElements = new List<int>();
        }

        public static string Dump(object element) {
            return Dump(element, 2);
        }

        public static string Dump(object element, int indentSize) {
            ObjectDumper instance = new ObjectDumper(indentSize);
            return instance.DumpElement(element);
        }

        private string DumpElement(object? element) {
            if (element == null || element is ValueType || element is string) {
                Write(FormatValue(element));
            } else if (element is IEnumerable enumerableElement) {
                Type objectType = element.GetType();

                if (enumerableElement == null) {
                    Write("{{{0}}}", objectType.FullName!);
                    FoundElements.Add(element.GetHashCode());
                    Level++;

                    MemberInfo[] members = element.GetType().GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    foreach (MemberInfo memberInfo in members) {
                        FieldInfo? fieldInfo = (memberInfo as FieldInfo);
                        PropertyInfo? propertyInfo = memberInfo as PropertyInfo;

                        Type type;
                        object? value;
                        if (fieldInfo is null)
                            { 
                            if (propertyInfo is null) {
                                continue;
                                
                            }else {
                                type = propertyInfo.PropertyType;
                                value = propertyInfo.GetValue(element, null);
                            }
                        } else {
                            type = fieldInfo.FieldType;
                            value= fieldInfo.GetValue(element);
                        }


                        if (type.IsValueType || type == typeof(string)) {
                            Write("{0}: {1}", memberInfo.Name, FormatValue(value));
                        } else {
                            bool isEnumerable = typeof(IEnumerable).IsAssignableFrom(type);
                            Write("{0}: {1}", memberInfo.Name, isEnumerable ? "..." : "{ }");

                            bool alreadyTouched = !isEnumerable && AlreadyTouched(value);
                            Level++;
                            if (!alreadyTouched) {
                                DumpElement(value);
                            } else {
                                Write("{{{0}}} <-- bidirectional reference found", value?.GetType()?.FullName?? "null");
                            }
                            Level--;
                        }
                    }
                } else {
                    foreach (object item in enumerableElement) {
                        if (item is IEnumerable && !(item is string)) {
                            Level++;
                            DumpElement(item);
                            Level--;
                        } else {
                            if (!AlreadyTouched(item)) {
                                DumpElement(item);
                            } else {
                                Write("{{{0}}} <-- bidirectional reference found", item.GetType().FullName!);
                            }
                        }
                    }
                }

                Level--;
            }
            else {
                Debug.Assert(false, "Should append");
            }
            return Builder.ToString();
        }

        private bool AlreadyTouched(object? value) {
            if (value == null)
                return false;

            int hash = value.GetHashCode();
            for (int i = 0; i < FoundElements.Count; i++) {
                if (FoundElements[i] == hash) {
                    return true;
                }
            }
            return false;
        }

        private void Write(string value, params object[] args) {
            string space = new string(' ', Level * IndentSize);

            if (args != null) {
                value = string.Format(value, args);
            }

            Builder.Append(space);
            Builder.AppendLine(value);
        }

        private string FormatValue(object? o) {
            if (o == null) {
                return "null";
            }

            if (o is DateTime dt) {
                return dt.ToShortDateString();
            }

            if (o is string) {
                return string.Format($"\"{o}\"");
            }

            if (o is char c && c == '\0') {
                return string.Empty;
            }

            if (o is ValueType) {
                return o.ToString() ?? "";
            }

            if (o is IEnumerable) {
                return "...";
            }

            return "{ }";
        }
    }
}
