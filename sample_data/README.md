### モデルについて
学習済みモデルのグラフデータを frozen_inference_graph.pb というファイル名で実行可能ファイルと同じディレクトリに置いてください。

自身で学習したモデルデータの場合、*.ckptファイルは読み込めないので export_inference_graph.py で出力してください。

とりあえず実行してみたい場合は、 ["Tensorflow detection model zoo
"](https://github.com/tensorflow/models/blob/master/research/object_detection/g3doc/detection_model_zoo.md#coco-trained-models) から適当なモデルをダウンロード／解凍し、その中にある frozen_inference_graph.pb を使用してください。

### ラベルについて
実行可能ファイルと同じディレクトリに labels.txt（UTF-16）を作成し、ラベル名を改行を区切りとして記述してください。

プログラム内では行番号をラベルIDとして扱います。  
e.g.  
`label_map.pbtxt`などが次の場合、
```
{
    id: 1
    name: "person"
}
{
    id: 2
    name: "bicycle"
}
```
`labels.txt`は次の通り。
```
person
bicycle
```

["Tensorflow detection model zoo
"](https://github.com/tensorflow/models/blob/master/research/object_detection/g3doc/detection_model_zoo.md#coco-trained-models) のモデルデータのラベルは /sample_data/labels.txt に置いてあります（たぶんこれで動くはず）。