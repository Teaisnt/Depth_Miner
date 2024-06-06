using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using static UnityEngine.Mathf;
using Color = UnityEngine.Color;
using Random = UnityEngine.Random;
using Image = UnityEngine.UI.Image;

public static class Extensions {
    public static void SetLayer(this GameObject obj, LayerMask layerMask, bool children) {
        obj.layer = layerMask;

        if (!children || obj.transform.childCount <= 0) return;

        Transform[] childObjects = obj.GetComponentsInChildren<Transform>();
        foreach (Transform child in childObjects) {
            child.gameObject.layer = layerMask;
        }
    }

    public static void SetLayer(this GameObject obj, string layerName, bool children) {
        LayerMask layerMask = LayerMask.NameToLayer(layerName);
        obj.layer = layerMask;

        if (!children || obj.transform.childCount <= 0) return;

        Transform[] childObjects = obj.GetComponentsInChildren<Transform>();
        foreach (Transform child in childObjects) {
            child.gameObject.layer = layerMask;
        }
    }

    public static Transform GetClosest(this Transform _transform, params Component[] array) {
        Transform closest = null;
        float checkDistance = Infinity;
        foreach (Component comp in array) {
            float distance = Vector3.Distance(_transform.position, comp.transform.position);
            if (distance < checkDistance) {
                checkDistance = distance;
                closest = comp.transform;
            }
        }

        return closest;
    }

    public static void SetColliderEnabled(this GameObject obj, bool enable, bool children) {
        obj.GetComponent<Collider>().enabled = enable;
        if (!children) return;

        Collider[] colliders = obj.GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders) {
            col.enabled = enable;
        }
    }

    public static void SetTag(this GameObject obj, string newTag, bool children) {
        obj.tag = newTag;

        if (!children) return;
        Transform[] childObjects = obj.GetComponentsInChildren<Transform>();
        foreach (Transform child in childObjects) {
            child.gameObject.tag = newTag;
        }
    }

    public static void AddComponent<T>(this GameObject obj, bool children) where T : Component {
        obj.AddComponent<T>();
        if (!children) return;

        foreach (Transform child in obj.transform) {
            child.gameObject.AddComponent<T>();
        }
    }

    public static T GetVariableFromString<T>(this Component comp, string varName) {
        return (T)comp.GetType().GetField(varName).GetValue(comp);
    }

    public static void MoveTowards(this Transform trans, Vector3 target, float maxDistanceDelta) {
        trans.position = Vector3.MoveTowards(trans.position, target, maxDistanceDelta);
    }

    public static Vector3 GetCenter(this GameObject obj) {
        List<float> x = new();
        List<float> y = new();
        List<float> z = new();
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers) {
            x.Add(renderer.bounds.center.x);
            y.Add(renderer.bounds.center.y);
            z.Add(renderer.bounds.center.z);
        }

        float ax = (Max(x.ToArray()) + Min(x.ToArray())) / 2;
        float ay = (Max(y.ToArray()) + Min(y.ToArray())) / 2;
        float az = (Max(z.ToArray()) + Min(z.ToArray())) / 2;
        return new Vector3(ax, ay, az);
    }

    public static Vector3 GetBottom(this GameObject obj) {
        MeshFilter[] meshFilters = obj.GetComponentsInChildren<MeshFilter>();

        Vector3 bottom = obj.GetCenter();
        Vector3 lowestPoint = meshFilters[0].mesh.vertices[0];

        foreach (MeshFilter meshFilter in meshFilters) {
            Mesh mesh = meshFilter.mesh;
            foreach (Vector3 vertex in mesh.vertices) {
                if (vertex.y < lowestPoint.y) {
                    lowestPoint = vertex;
                }
            }
        }

        bottom.y = lowestPoint.y;
        return bottom;
    }

    public static Vector3 GetCenterNew(this GameObject obj) {
        Bounds bounds = new();
        bool first = true;
        foreach (Transform child in obj.transform) {
            if (child.GetComponent<MeshRenderer>()) {
                if (first) {
                    first = false;
                    bounds = child.GetComponent<MeshRenderer>().bounds;
                } else {
                    bounds.Encapsulate(child.GetComponent<MeshRenderer>().bounds);
                }
            }
        }

        return bounds.center;
    }

    public static Transform[] GetAllChildren(this Transform trans) {
        return trans.GetComponentsInChildren<Transform>();
    }

    public static void SetAlpha(this Image image, float alpha) {
        Color colour = image.color;
        colour.a = alpha;
        image.color = colour;
    }

    public static void SetAlpha(this Material mat, float alpha) {
        Color colour = mat.color;
        colour.a = alpha;
        mat.color = colour;
    }

    public static void SetX(this Transform trans, float newX, bool relative) {
        Vector3 pos = trans.position;
        pos.x = relative ? pos.x + newX : newX;
        trans.position = pos;
    }

    public static void SetY(this Transform trans, float newY, bool relative) {
        Vector3 pos = trans.position;
        pos.y = relative ? pos.y + newY : newY;
        trans.position = pos;
    }

    public static void SetZ(this Transform trans, float newZ, bool relative) {
        Vector3 pos = trans.position;
        pos.z = relative ? pos.z + newZ : newZ;
        trans.position = pos;
    }

    public static bool IsSimilar(this Material mat, Material other) {
        Texture2D matTex = (Texture2D)mat.mainTexture;
        Texture2D otherTex = (Texture2D)other.mainTexture;
        //matTex.Apply();
        //otherTex.Apply();

        if (mat.mainTexture == null && other.mainTexture == null) {
            return mat.color == other.color;
        }

        return mat.color == other.color && matTex == otherTex;
    }
}

public class Ext {
    public static void LoadSceneOffline(string sceneName) {
        SceneManager.LoadScene(sceneName);
    }

    public static Scene GetActiveScene() {
        return SceneManager.GetActiveScene();
    }

    public static int CryptoRandomInt(int min, int max) {
        return RandomNumberGenerator.GetInt32(min, max);
    }

    public static float CryptoRandomFloat(float min, float max) {
        var rng = RandomNumberGenerator.Create();

        byte[] bytes = new byte[sizeof(float)];
        rng.GetBytes(bytes);

        float randomFloat = BitConverter.ToSingle(bytes, 0);
        randomFloat = (randomFloat * (max - min)) + min;
        return randomFloat;
    }

    public static IEnumerator WaitAndDo(Action firstAction, float time, Action secondAction) {
        firstAction.Invoke();
        yield return new WaitForSeconds(time);
        secondAction.Invoke();
    }

    public static IEnumerator WaitAndDo(Func<bool> condition, Action action) {
        while (condition.Invoke()) {
            yield return null;
        }
        action.Invoke();
    }

    public static IEnumerator CheckForInternet(Action connected, Action notConnected) {
        using UnityWebRequest request = UnityWebRequest.Get("https://www.google.com/");
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success) {
            // Has Internet
            connected.Invoke();
        } else {
            // Doesn't have internet
            notConnected.Invoke();
        }
    }
}

public class MathExt {
    public static float Wrap(float value, float min, float max) {
        if (value % 1 == 0) {
            while (value > max || value < min) {
                if (value > max) {
                    value += min - max - 1;
                } else if (value < min) {
                    value += max - min + 1;
                }
            }
            return value;
        } else {
            float vOld = value + 1;
            while (value != vOld) {
                vOld = value;
                if (value < min) {
                    value = max - (min - value);
                } else if (value > max) {
                    value = min + (value - max);
                }
            }
            return value;
        }
    }

    public static int Wrap(int value, int min, int max) {
        if (value % 1 == 0) {
            while (value > max || value < min) {
                if (value > max) {
                    value += min - max - 1;
                } else if (value < min) {
                    value += max - min + 1;
                }
            }
            return value;
        } else {
            int vOld = value + 1;
            while (value != vOld) {
                vOld = value;
                if (value < min) {
                    value = max - (min - value);
                } else if (value > max) {
                    value = min + (value - max);
                }
            }
            return value;
        }
    }

    public static bool Chance(float chance) {
        return chance > Random.Range(0f, 1f);
    }

    public static bool CryptoChance(float chance) {
        return chance > Ext.CryptoRandomFloat(0f, 1f);
    }

    public static T Choose<T>(params T[] array) {
        int max = array.Length;
        return array[Random.Range(0, max)];
    }

    public static T Choose<T>(List<T> list) {
        return Choose(list.ToArray());
    }

    public static T CryptoChoose<T>(params T[] array) {
        int max = array.Length;
        return array[Ext.CryptoRandomInt(0, max)];
    }

    public static T CryptoChoose<T>(List<T> list) {
        return CryptoChoose(list.ToArray());
    }
}