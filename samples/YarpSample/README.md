# YarpSample

一个集成反向代理 [YARP](https://github.com/microsoft/reverse-proxy) 的例子

## 说明

### 一些配置

nacos-sdk-csharp 提供了查询服务列表的分页方法可供查询注册到 nacos server 里面指定命名空间某个分组下面的服务。

所以需要在启动之前指定对应的分组信息【GroupNameList】，和每一页的页数【PreCount】，建议这个页数设置的大一点。

这个时候可以很好的应对现存服务的实例上下线情况，但是对新的服务和要下线某个服务就没有好办法了。

针对新增和减少服务的情况，提供了一个定时查询的开关【EnableAutoRefreshService】，以及查询的间隔【AutoRefreshPeriod】

> PS: 服务和实例要区分开。

### 转发说明

默认的转发策略规则举例

- 反向代理： http://10.0.0.99:5000
- 服务A : servicename = app1, groupname = mygroup
    - 实例1： http://10.0.0.2:8008
    - 实例2： http://10.0.0.2:8009
- 服务B : servicename = app2, groupname = mygroup
    - 实例1： http://10.0.0.3:9008
    - 实例2： http://10.0.0.3:9009


通过反向代理访问服务A

```
curl http://10.0.0.99:5000/app1/xxxxx
```

通过反向代理访问服务B

```
curl http://10.0.0.99:5000/app2/xxxxx
```

规则是 【反向代理地址/服务的servicename/服务的相对地址】

> PS: 避免出现不同 groupname，相同 servicename 的情况！！

如果转发策略不满足，实现 **INacosYarpConfigMapper** 去替换默认的即可。