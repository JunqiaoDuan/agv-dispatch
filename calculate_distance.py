#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
计算两个地点经纬度之间的距离

使用Haversine公式计算地球表面两点之间的距离

示例坐标数据:
(120.80157568, 30.36124268)
(120.80154368, 30.36122002)
(120.80157603, 30.36123931)
(120.80157603, 30.36123929)
"""

import math
import re


# 示例坐标数据
SAMPLE_COORDINATES = [
    (120.80157568, 30.36124268),  # 基准点
    (120.80154368, 30.36122002),
    (120.80157603, 30.36123931),
    (120.80157603, 30.36123929),
]


def calculate_xy_distance(lat1, lon1, lat2, lon2):
    """
    计算两个经纬度坐标之间的X、Y方向距离（单位：米）

    参数:
        lat1: 第一个点的纬度
        lon1: 第一个点的经度
        lat2: 第二个点的纬度
        lon2: 第二个点的经度

    返回:
        (x, y) 元组：
        x: 东西方向距离（米），正值表示向东，负值表示向西
        y: 南北方向距离（米），正值表示向北，负值表示向南
    """
    # 地球半径（米）
    R = 6371000

    # 将角度转换为弧度
    lat1_rad = math.radians(lat1)
    lat2_rad = math.radians(lat2)
    delta_lat = lat2 - lat1
    delta_lon = lon2 - lon1

    # 计算Y方向距离（南北方向，纬度差）
    y = R * math.radians(delta_lat)

    # 计算X方向距离（东西方向，经度差）
    # 需要考虑纬度的影响，使用平均纬度
    avg_lat_rad = (lat1_rad + lat2_rad) / 2
    x = R * math.radians(delta_lon) * math.cos(avg_lat_rad)

    return (x, y)


def parse_coordinates(line):
    """
    解析坐标字符串，格式: (经度, 纬度)

    参数:
        line: 输入字符串，例如 "(120.80157568, 30.36124268)"

    返回:
        (lon, lat) 元组，如果解析失败返回None
    """
    # 匹配格式: (数字, 数字)
    pattern = r'\(?\s*([\d.]+)\s*,\s*([\d.]+)\s*\)?'
    match = re.search(pattern, line.strip())
    if match:
        lon = float(match.group(1))
        lat = float(match.group(2))
        return (lon, lat)
    return None


def calculate_relative_distances():
    """
    计算相对于基准点的距离
    第一行作为基准点，计算其他点到基准点的距离（单位：毫米）
    """
    print("=" * 60)
    print("经纬度距离计算器 - 相对距离模式")
    print("=" * 60)
    print("\n输入格式: (经度, 纬度)")
    print("第一行作为基准点，计算其他点相对于基准点的距离（单位：毫米）")
    print("输入空行结束输入\n")
    print("-" * 60)

    coordinates = []
    line_num = 1

    while True:
        try:
            line = input(f"点 {line_num}: ").strip()
            if not line:
                break

            coord = parse_coordinates(line)
            if coord:
                coordinates.append(coord)
                if line_num == 1:
                    print(f"  已设置为基准点: 经度 {coord[0]}, 纬度 {coord[1]}")
                else:
                    print(f"  已添加: 经度 {coord[0]}, 纬度 {coord[1]}")
                line_num += 1
            else:
                print("  解析失败，请重新输入")
        except KeyboardInterrupt:
            print("\n\n程序已退出")
            return

    if len(coordinates) < 2:
        print("\n至少需要输入2个坐标点（基准点 + 至少1个对比点）")
        return

    # 第一个点作为基准
    base_lon, base_lat = coordinates[0]

    print("\n" + "=" * 60)
    print("计算结果")
    print("=" * 60)
    print(f"基准点: 经度 {base_lon}, 纬度 {base_lat}\n")

    for i, (lon, lat) in enumerate(coordinates[1:], start=2):
        distance_m = haversine_distance(base_lat, base_lon, lat, lon)
        distance_mm = distance_m * 1000  # 转换为毫米
        print(f"点 {i}: 经度 {lon}, 纬度 {lat}")
        print(f"  → 相对基准点距离: {distance_mm:.2f} mm ({distance_m:.6f} m)\n")


def format_distance(distance_meters):
    """
    格式化距离显示

    参数:
        distance_meters: 距离（米）

    返回:
        格式化的距离字符串
    """
    if distance_meters < 1000:
        return f"{distance_meters:.2f} 米"
    else:
        return f"{distance_meters / 1000:.2f} 千米"


if __name__ == "__main__":
    import sys

    # 默认使用示例坐标数据进行计算
    print("=" * 60)
    print("经纬度距离计算器 - X/Y方向距离")
    print("=" * 60)

    base_lon, base_lat = SAMPLE_COORDINATES[0]
    print(f"\n基准点: 经度 {base_lon}, 纬度 {base_lat}\n")
    print("-" * 60)

    for i, (lon, lat) in enumerate(SAMPLE_COORDINATES[1:], start=2):
        x, y = calculate_xy_distance(base_lat, base_lon, lat, lon)
        x_mm = x * 1000  # 转换为毫米
        y_mm = y * 1000  # 转换为毫米

        print(f"点 {i}: 经度 {lon}, 纬度 {lat}")
        print(f"  X方向(东西): {x_mm:>10.2f} mm ({x:>10.6f} m)")
        print(f"  Y方向(南北): {y_mm:>10.2f} mm ({y:>10.6f} m)")
        print()
