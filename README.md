# TextMeshGC Optimize
TextMesh堆内存优化

### 问题简介
TextMeshPro(TMP)是Unity的字体插件，具有很强大的功能，然而使用的过程中会发现堆内存的申请频率和大小都不低，尤其是在UI界面的使用上，每次打开界面，都会使得界面上的TMP进行初始化，随着使用时长的增加，就容易引起GC。（当前版本 com.unity.textmeshpro@1.4.1）

### 问题分析
![image](https://github.com/FallingXun/TextMeshGCOptimize/blob/main/Images/1.png)  
创建50个TMP文本时，会有1.1MB的堆内存申请，从图中可以看出：   
1. Object.Instantiate()：0.8MB   
- TextMeshProUGUI..ctor: 301.2KB   
- TMP_TextInfo..ctor: 178.9KB   
2. GameObject.SetActive(): 368.2KB   
- TextMeshProUGUI.Awake(): 266.8KB
- TextMeshProUGUI.OnEnable(): 100.3KB   

也就是说，主要的堆内存来源于 **TextMeshProUGUI** 和 **TMP_TextInfo** 的构造和初始化方法里，查看代码可知，主要原因是由于**TMP_Text** 和 **TMP_TextInfo** 在创建时预创建了较多的数组变量，导致申请了较大的堆内存，另一方面，数组变量在中间使用的过程，存在Resize操作，又会产生新的堆内存申请。  

因此，如果我们每次在销毁的时候，将原有申请的内存缓存起来，在下一次创建的时候重新拿出来使用，则可以避免每次都重新申请的问题，即使用对象池。

### 优化流程

TMP中有对象池的模式，TMP_ListPool中，提供了List<T>的对象池结构，TMP其中也有使用到此结构，如果同样使用此结构，则需要将原来使用数组的地方全部改为List，显然会有非常大的工作量，而且非常容易产生难以排查的bug。

因此，为了尽可能降低复杂度，这里使用了 Dictionary + Stack + Array 的对象池结构，即 Dictionary<int, Stack<T[]>>，Dictionary使用Array.Length作为索引key值，不同类型不同长度的数组放进不同的栈里保存，由于TMP预定义的数组长度大部分是定长的，所以大部分进入池中的数组复用率将会比较高。

由于保持了原有的数组的模式，所以对原逻辑的修改将会降到最小，只需将new Array的地方改为对象池即可。如：
- 修改前：characterInfo = new TMP_CharacterInfo[8];
- 修改后：TMP_ArrayPool<TMP_CharacterInfo>.Release(characterInfo);   
            characterInfo = TMP_ArrayPool<TMP_CharacterInfo>.Get(8);

先回收原有的数组，再从池里提取新的长度的对象。每次提取前先释放，为了避免出现数组没回池。例如 TMP 中原本存在的问题：  
- 当使用 Instantiate(tmp) 时，实例化完成后，TMP_TextInfo() 会执行一次，预创建对应的数组，而在Awake()方法中，会再执行 m_textInfo = new TMP_TextInfo(this); 即创建一个tmp的过程中会触发两次构造函数，创建了两次数组变量，如果没进行先回收再提取，则每次都还是会重新创建新的数组。（经测试，单个TMP此处进行回收后可以减少3.2KB的堆内存申请）

然而，对于 TMP_TextInfo.Resize<T>(ref T[] array, int size) 方法，则与上述相反，需要先从池里提取新的长度的数组，再进行数组回收。因为旧数组的数据需要拷贝到新数组里，如果先执行回收，则会清除旧数组的数据，所以这里需要调换执行顺序。

### 测试数据
#### 首次创建50个
![无对象池首次创建50个](https://github.com/FallingXun/TextMeshGCOptimize/blob/main/Images/无对象池首次创建50个.png)
- 无对象池首次创建50个

![有对象池首次创建50个](https://github.com/FallingXun/TextMeshGCOptimize/blob/main/Images/有对象池首次创建50个.png)
- 有对象池首次创建50个

> 首次创建50个时，开启对象池后，TextMeshProUGUI.Awake()方法降低了157KB，主要来自前面说的 TextInfo 二次构造产生的堆内存。

#### 首次销毁50个
![无对象池首次销毁50个](https://github.com/FallingXun/TextMeshGCOptimize/blob/main/Images/无对象池首次销毁50个.png)
- 无对象池首次销毁50个

![有对象池首次销毁50个](https://github.com/FallingXun/TextMeshGCOptimize/blob/main/Images/有对象池首次销毁50个.png)
- 有对象池首次销毁50个

> 首次销毁50个时，开启对象池后，由于创建了池，所以会产生额外的内存占用。

#### 二次创建50个
![无对象池二次创建50个](https://github.com/FallingXun/TextMeshGCOptimize/blob/main/Images/无对象池二次创建50个.png)
- 无对象池二次创建50个

![有对象池二次创建50个](https://github.com/FallingXun/TextMeshGCOptimize/blob/main/Images/有对象池二次创建50个.png)
- 有对象池二次创建50个

> 二次创建50个时，开启对象池后，TextMeshProUGUI..ctor()从301.2KB降到48KB，TextMeshProUGUI.Awake()从178.9KB降到0KB，如果多次销毁创建，则累计能节省很大的堆内存占用。

#### 二次销毁50个
![无对象池二次销毁50个](https://github.com/FallingXun/TextMeshGCOptimize/blob/main/Images/无对象池二次销毁50个.png)
- 无对象池二次销毁50个

![有对象池二次销毁50个](https://github.com/FallingXun/TextMeshGCOptimize/blob/main/Images/有对象池二次销毁50个.png)
- 有对象池二次销毁50个

### 总结
TMP的堆内存占用，还是相对比较明显的，尤其是在频繁创建销毁的UI界面上，长时间开关界面将会累计较高的内存占用，从而容易引起GC触发，通过使用对象池，能有效地避免这一问题。

### 后续优化
1. TMP_MeshInfo中有较多的数组创建，可进行对象池处理
2. 检查堆内存占用较大的方法，可根据情况进行细致检查
3. 由于对象池会把内存长期占用，可增加管理器进行按需释放