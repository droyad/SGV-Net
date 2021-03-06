﻿using CK.Core;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleGitVersion.DNX.Tests
{
    [TestFixture]
    public class JSONVisitorTests
    {
        class JSONProperties : JSONVisitor
        {
            public List<string> Properties;
            public List<string> Paths;

            public JSONProperties( StringMatcher m)
                : base( m )
            {
                Properties = new List<string>();
                Paths = new List<string>();
            }

            public override bool VisitObjectProperty( int startPropertyIndex, string propertyName, int propertyIndex )
            {
                Properties.Add( propertyName );
                Paths.Add( string.Join( "|", Path.Select( x => x.Index + "=" + x.PropertyName ) ) + " => " + propertyIndex + "=" + propertyName );
                return base.VisitObjectProperty( startPropertyIndex, propertyName, propertyIndex );
            }
        }

        [Test]
        public void json_visit_all_properties()
        {
            string s = @"
{ 
    ""p1"": ""n"", 
    ""p2"": 
    { 
        ""p3"": 
        [ 
            {
                ""p4Before"": [""zero""  , ""one""
                                , { ""pSub"": [] }, 
                                ""three"" ]
                ""p4"": 
                { 
                    ""p5"" : 0.989, 
                    ""p6"": [],
                    ""p7"": {}
                }
            }
        ] 
    } 
}";
            JSONProperties p = new JSONProperties( new StringMatcher( s ) );
            p.Visit();
            CollectionAssert.AreEqual( new[] { "p1", "p2", "p3", "p4Before", "pSub", "p4", "p5", "p6", "p7" }, p.Properties );
            CollectionAssert.AreEqual( new[] {
                " => 0=p1",
                " => 1=p2",
                "1=p2 => 0=p3",
                "1=p2|0=p3|0= => 0=p4Before",
                "1=p2|0=p3|0=|0=p4Before|2= => 0=pSub",
                "1=p2|0=p3|0= => 1=p4",
                "1=p2|0=p3|0=|1=p4 => 0=p5",
                "1=p2|0=p3|0=|1=p4 => 1=p6",
                "1=p2|0=p3|0=|1=p4 => 2=p7" }, p.Paths );
        }

        [TestCase( @"null, true", null, ", true" )]
        [TestCase( @"""""X", "", "X" )]
        [TestCase( @"""a""X", "a", "X" )]
        [TestCase( @"""\\""X", @"\", "X" )]
        [TestCase( @"""A\\B""X", @"A\B", "X" )]
        [TestCase( @"""A\\B\r""X", "A\\B\r", "X" )]
        [TestCase( @"""A\\B\r\""""X", "A\\B\r\"", "X" )]
        [TestCase( @"""\u8976""X", "\u8976", "X" )]
        [TestCase( @"""\uABCD\u07FC""X", "\uABCD\u07FC", "X" )]
        [TestCase( @"""\uabCd\u07fC""X", "\uABCD\u07FC", "X" )]
        public void string_matcher_TryMatchJSONQuotedString( string s, string parsed, string textAfter )
        {
            var m = new StringMatcher( s );
            string result;
            Assert.That( m.TryMatchJSONQuotedString( out result, true ) );
            Assert.That( result, Is.EqualTo( parsed ) );
            Assert.That( m.TryMatchText( textAfter ), "Should be followed by: " + textAfter );
            
            m = new StringMatcher( s );
            Assert.That( m.TryMatchJSONQuotedString( true ) );
            Assert.That( m.TryMatchText( textAfter ), "Should be followed by: " + textAfter );
        }
    }
}
