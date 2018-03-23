# Help12306

本程序通过抓包分析模拟了12306的整个购票流程，请使用vs2010打开本项目，将info.xml复制到程序生成目录和exe同级，然后启动exe，info中包含登录信息和乘客信息。<br>
由于目前客户端采用了加密机制，我并没有分析加密算法，所以用webbrowser控件直接去加载页面执行脚本，后续过程全部通过模拟发包的方式，截至2018年3月23日 10：00能成功购票，查询车票使用了多线程，尽最大可能捡漏，可配置起始车站，默认只抢动车或高铁票。

#### 需要注意的地方：
    1.在选择目标车次这块并没有做成配置，可根据需求直接修改代码
    2.并不能识别验证码，在登录的时候需要手动输入
    3.城市数据配置在代码的字典里，如果没有你的城市，需要在代码中添加映射，访问12306官网选择你要的城市，f12看属性
    
#### 热门城市列表
    <li class="ac_even openLi ac_odd" title="北京" data="BJP">北京</li>
    <li class="ac_even openLi ac_odd" title="上海" data="SHH">上海</li>
    <li class="ac_even openLi ac_odd" title="天津" data="TJP">天津</li>
    <li class="ac_even openLi ac_odd" title="重庆" data="CQW">重庆</li>
    <li class="ac_even openLi ac_odd" title="长沙" data="CSQ">长沙</li>
    <li class="ac_even openLi ac_odd" title="长春" data="CCT">长春</li>
    <li class="ac_even openLi ac_odd" title="成都" data="CDW">成都</li>
    <li class="ac_even openLi ac_odd" title="福州" data="FZS">福州</li>
    <li class="ac_even openLi ac_odd" title="广州" data="GZQ">广州</li>
    <li class="ac_even openLi ac_odd" title="贵阳" data="GIW">贵阳</li>
    <li class="ac_even openLi ac_odd" title="呼和浩特" data="HHC">呼和浩特</li>
    <li class="ac_even openLi ac_odd" title="哈尔滨" data="HBB">哈尔滨</li>
    <li class="ac_even openLi ac_odd" title="合肥" data="HFH">合肥</li>
    <li class="ac_even openLi ac_odd" title="杭州" data="HZH">杭州</li>
    <li class="ac_even openLi ac_odd" title="海口" data="VUQ">海口</li>
    <li class="ac_even openLi ac_odd" title="济南" data="JNK">济南</li>
    <li class="ac_even openLi ac_odd" title="昆明" data="KMM">昆明</li>
    <li class="ac_even openLi ac_odd" title="拉萨" data="LSO">拉萨</li>
    <li class="ac_even openLi ac_odd" title="兰州" data="LZJ">兰州</li>
    <li class="ac_even openLi ac_odd" title="南宁" data="NNZ">南宁</li>
    <li class="ac_even openLi ac_odd" title="南京" data="NJH">南京</li>
    <li class="ac_even openLi ac_odd" title="南昌" data="NCG">南昌</li>
    <li class="ac_even openLi ac_odd" title="沈阳" data="SYT">沈阳</li>
    <li class="ac_even openLi ac_over" title="石家庄" data="SJP">石家庄</li>
    <li class="ac_even openLi ac_odd" title="太原" data="TYV">太原</li>
    <li class="ac_even openLi ac_odd" title="乌鲁木齐南" data="WMR">乌鲁木齐南</li>
    <li class="ac_even openLi ac_odd" title="武汉" data="WHN">武汉</li>
    <li class="ac_even openLi ac_odd" title="西宁" data="XNO">西宁</li>
    <li class="ac_even openLi ac_odd" title="西安" data="XAY">西安</li>
    <li class="ac_even openLi ac_odd" title="银川" data="YIJ">银川</li>
    <li class="ac_even openLi ac_odd" title="郑州" data="ZZF">郑州</li>
    <li class="ac_even openLi ac_odd" title="深圳" data="SZQ">深圳</li>
    <li class="ac_even openLi ac_odd" title="厦门" data="XMS">厦门</li>


我只是将整个过程模拟了一遍，在使用过程中你可能需要修改部分代码，你也可以参考提交地址和数据结构，如果有时间我也会继续优化，同时欢迎大家pull request<br>
如果你成功抢到了回家的票，请不要吝啬给我一颗star
