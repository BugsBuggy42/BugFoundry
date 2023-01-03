namespace BugFoundry.BugFoundryGame.Shared
{
    public class DefaultCode
    {
        public static string[] Default = new[]
        {
            "using UnityEngine;",
            "",
            "public class Ref : MonoBehaviour",
            "{",
            "    private void Start()",
            "    {",
            "",
            "    }",
            "",
            "    private void Update()",
            "    {",
            "",
            "    }",
            "}",
            "",
        };

        public static string[] Level1 = new[]
        {
            "using UnityEngine;",
            "",
            "public class Ref : MonoBehaviour",
            "{",
            "    GameObject capsule;",
            "",
            "    private void Start()",
            "    {",
            "        capsule = GameObject.Find(\"Capsule\");",
            "    }",
            "",
            "    private void Update()",
            "    {",
            "",
            "    }",
            "}",
            "",
        };

        public static string[] Level2 = new[]
        {
            "using UnityEngine;",
            "",
            "public class Ref : MonoBehaviour",
            "{",
            "    public float speed = 2;",
            "    public GameObject capsule;",
            "",
            "    private void Start()",
            "    {",
            "        this.capsule = GameObject.Find(\"Capsule\");",
            "        // Do not modify the code above",
            "",
            "        // Don not modify the code below",
            "    }",
            "",
            "    private void Update()",
            "    {",
            "        capsule.transform.position += Vector3.forward * speed * Time.deltaTime;",
            "    }",
            "}",
            "",
        };
    }
}