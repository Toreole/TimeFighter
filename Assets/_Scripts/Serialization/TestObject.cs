using System;
using System.Collections.Generic;
using Game.Serialization;

namespace Game.Serialization.Testing
{
    public class TestObject : SerializedMonoBehaviour
    {
        protected string someRandomText = "hello";

        public override void Deserialize(ObjectData data)
        {
            var m_data = data as TestObjectData;
        }

        public override ObjectData Serialize()
        {
            var data = new TestObjectData
            {
                myText = someRandomText
            };
            return data;
        }

        public void SetOtherText(string text)
        {
            someRandomText = text;
        }

        public class TestObjectData : ObjectData
        {
            public string myText;
        }
    }
}
