using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class VolumeTrailRenderer : MonoBehaviour {

    [Serializable]
    public class TubeVertex {
        public float timeCreated = 0.00f;
        public Vector3 point = Vector3.zero;
        public float radius = 1.0f;
        public Color color = Color.white;

        public float timeAlive {
            get {return Time.time - timeCreated;}
        }

        public TubeVertex() {
		}

        public TubeVertex(Vector3 pt, float r, Color c) {
            point = pt;
            radius = r;
            color = c;
        }
    }

    public bool emit = true;
    public float emitTime = 0.00f;
    public bool autoDestruct = false;
    public Material material;
    public Transform target;
    [SerializeField] Vector3 offset;
    public int crossSegments = 18;
    public AnimationCurve radius;
    public Gradient colorOverLifeTime;
    public float lifeTime;

    [SerializeField] float fadeSpeed = 2f;

	public List<TubeVertex> vertices;
	private Color[] vertexColors;

    private MeshRenderer meshRenderer;
    private Vector3[] crossPoints;
    private int lastCrossSegments;

    internal bool fadeOut;
    internal float fadeBias;

    void Reset() {
    	GetComponent<MeshFilter>().hideFlags = HideFlags.HideInInspector;
        meshRenderer.hideFlags = HideFlags.HideInInspector;

        vertices = new List<TubeVertex>() {
            new TubeVertex(Vector3.zero, 1.0f, Color.white),
            new TubeVertex(new Vector3(1,0,0), 1.0f, Color.white),
        };
    }

    void Start() {
        meshRenderer = gameObject.GetComponent<MeshRenderer>();
        meshRenderer.material = material;
    }

    void Update() {
    	//fadeBias = (fadeOut ? Mathf.MoveTowards(fadeBias, 0.0f, Time.deltaTime * fadeSpeed) : 1);
    	if (fadeOut) fadeBias -= Time.deltaTime * fadeSpeed;
    	fadeBias = Mathf.Clamp01((fadeOut ? fadeBias : 1.0f));
    }

    void LateUpdate() {
		transform.position = Vector3.zero;
		transform.rotation = Quaternion.identity;
	
        GetComponent<Renderer>().enabled = vertices.Count <= 2 ? false : true;

        if (emit && emitTime >= 0) {
            emitTime -= Time.deltaTime;
            if (emitTime <= 0) {
				emitTime = -1;
			}
            if (emitTime < 0) {
				emit = false;
			}
        }

        // destroy if autodestruct.
        if (!emit && vertices.Count == 0 && autoDestruct) {
            Destroy(gameObject);
        }

        if (emit) {
			TubeVertex p = new TubeVertex();
			p.point = target.position + target.TransformDirection(offset);
			p.timeCreated = Time.time;
			vertices.Add(p);
        }

		ArrayList remove = new ArrayList();
		int i = 0;
		foreach (TubeVertex p in vertices) {
			// cull old points first
			if (Time.time - p.timeCreated > lifeTime) {
				remove.Add(p);
			}
			i++;
		}

		foreach (TubeVertex p in remove) {
			vertices.Remove(p);
		}
		remove.Clear();

		if (vertices.Count > 1) {

			//draw tube
			for (int k = 0; k < vertices.Count; k++) {
				vertices[k].radius = radius.Evaluate((float)((float)(vertices.Count - 1 - k) / (float)(vertices.Count - 1)));
			}

			if (crossSegments != lastCrossSegments) {
				crossPoints = new Vector3[crossSegments];
				float theta = 2.0f * Mathf.PI / crossSegments;
				for (int c = 0; c < crossSegments; c++) {
					crossPoints[c] = new Vector3(Mathf.Cos(theta * c), Mathf.Sin(theta * c), 0);
				}
				lastCrossSegments = crossSegments;
			}

			Vector3[] meshVertices = new Vector3[vertices.Count * crossSegments];
			Vector2[] uvs = new Vector2[vertices.Count * crossSegments];
			Color[] colors = new Color[vertices.Count * crossSegments];
			int[] tris = new int[vertices.Count * crossSegments * 6];
			int[] lastVertices = new int[crossSegments];
			int[] theseVertices = new int[crossSegments];
			Quaternion rotation = Quaternion.identity;

			for (int p = 0; p < vertices.Count; p++) {

				if (p < vertices.Count - 1) {
					rotation = Quaternion.FromToRotation(Vector3.forward, vertices[p + 1].point - vertices[p].point);
				}

				for (int c = 0; c < crossSegments; c++) {
					int vertexIndex = p * crossSegments + c;
					meshVertices[vertexIndex] = vertices[p].point + rotation * crossPoints[c] * vertices[p].radius;
					uvs[vertexIndex] = new Vector2((0f + c) / crossSegments, (1f + p) / vertices.Count);

					float overlifetime = (float)((float)(vertices.Count - 1 - p) / (float)(vertices.Count - 1));

					Color color = colorOverLifeTime.Evaluate(overlifetime) * fadeBias;
					colors[vertexIndex] = color;//vertices[p].color;

					lastVertices[c] = theseVertices[c];
					theseVertices[c] = p * crossSegments + c;	
				}
                
				//make triangles
                if (p > 0) {
                    for (int c = 0; c < crossSegments; c++) {
                        int start = (p * crossSegments + c) * 6;
                        tris[start] = lastVertices[c];
                        tris[start + 1] = lastVertices[(c + 1) % crossSegments];
						tris[start + 2] = theseVertices[c];
                        tris[start + 3] = tris[start + 2];
						tris[start + 4] = tris[start + 1];
						tris[start + 5] = theseVertices[(c + 1) % crossSegments];
					}
				}
			}

			Mesh mesh = GetComponent<MeshFilter>().mesh;
			if (!mesh) {
				mesh = new Mesh();
			}
			mesh.Clear();
			mesh.vertices = meshVertices;
			mesh.triangles = tris;
			mesh.colors = colors;

			mesh.RecalculateNormals();
			mesh.uv = uvs;
        }
    }

    public void Emit(float lifetime = 0.5f) {
        emit = true;
        lifeTime = lifetime;
        fadeBias = 1.0f;
    }

    void CalculateVertexColors() {
        vertexColors = new Color[vertices.Count * crossSegments];


        for (int i = 0; i < vertices.Count; i++) {
            vertexColors[i] = Color.white;
        }

        int divisions = crossSegments;

        for (int i = 0; i < divisions; i++) {
            vertexColors[i].a = 0;

            if (vertices.Count > divisions) {
                vertexColors[divisions + i] = new Color(1, 1, 1, 1f);
            }

            if (vertices.Count > divisions * 4) {
                vertexColors[divisions + i] = new Color(1, 1, 1, 1f);
            }

            vertexColors[vertices.Count * i].a = 0;
        }
    }
}