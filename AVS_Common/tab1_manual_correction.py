import sys
import os
import math
from PySide6.QtCore import Qt, QRectF
from PySide6.QtGui import QPen, QBrush, QColor, QPainter, QPixmap
from PySide6.QtWidgets import (QApplication, QMainWindow, QWidget, QHBoxLayout,
                               QVBoxLayout, QPushButton, QListWidget, QGraphicsView,
                               QGraphicsScene, QGraphicsItem, QGraphicsObject,
                               QGraphicsPixmapItem, QMessageBox, QLabel)


# ==========================================
# 工具函数：YOLO OBB 数学坐标转换核心
# ==========================================
def yolo_to_obb(yolo_coords, img_w, img_h):
    """ 将 YOLO 归一化的 8 个点转换为界面的 cx, cy, w, h, angle """
    pts = [(yolo_coords[i] * img_w, yolo_coords[i + 1] * img_h) for i in range(0, 8, 2)]

    # 计算中心点
    cx = sum([p[0] for p in pts]) / 4
    cy = sum([p[1] for p in pts]) / 4

    # 假设点是按顺时针排列的，计算宽、高和角度
    dx = pts[1][0] - pts[0][0]
    dy = pts[1][1] - pts[0][1]
    angle_rad = math.atan2(dy, dx)
    angle_deg = math.degrees(angle_rad)

    w = math.hypot(dx, dy)
    h = math.hypot(pts[2][0] - pts[1][0], pts[2][1] - pts[1][1])

    return cx, cy, w, h, angle_deg


def obb_to_yolo(cx, cy, w, h, angle_deg, img_w, img_h):
    """ 将界面的 cx, cy, w, h, angle 转换为 YOLO 的 8 个点归一化坐标 """
    angle_rad = math.radians(angle_deg)
    cos_a = math.cos(angle_rad)
    sin_a = math.sin(angle_rad)

    # 矩形未旋转前的四个顶点相对中心点的坐标
    corners = [
        (-w / 2, -h / 2), (w / 2, -h / 2),
        (w / 2, h / 2), (-w / 2, h / 2)
    ]

    yolo_coords = []
    for x, y in corners:
        # 旋转矩阵平移
        rx = x * cos_a - y * sin_a + cx
        ry = x * sin_a + y * cos_a + cy
        # 归一化限制在 0~1 之间
        yolo_coords.append(max(0.0, min(1.0, rx / img_w)))
        yolo_coords.append(max(0.0, min(1.0, ry / img_h)))

    return yolo_coords


# ==========================================
# 交互图元（复用之前的高级 OBB 绘图代码）
# ==========================================
class RotationHandle(QGraphicsItem):
    def __init__(self, parent=None):
        super().__init__(parent)
        self.radius = 6
        self.setFlags(QGraphicsItem.GraphicsItemFlag.ItemIsMovable | QGraphicsItem.GraphicsItemFlag.ItemSendsGeometryChanges)

    def boundingRect(self):
        return QRectF(-self.radius, -self.radius, self.radius * 2, self.radius * 2)

    def paint(self, painter, option, widget=None):
        painter.setPen(QPen(QColor(255, 165, 0), 1, Qt.PenStyle.DashLine))
        painter.drawLine(0, self.radius, 0, 25)
        painter.setPen(QPen(QColor(255, 69, 0), 2))
        painter.setBrush(QBrush(QColor(255, 255, 255)))
        painter.drawEllipse(-self.radius, -self.radius, self.radius * 2, self.radius * 2)

    def mouseMoveEvent(self, event):
        parent = self.parentItem()
        if parent:
            mouse_in_parent = self.mapToParent(event.pos())
            angle_rad = math.atan2(mouse_in_parent.x(), -mouse_in_parent.y())
            parent.setRotation(math.degrees(angle_rad))


class SizeHandle(QGraphicsItem):
    def __init__(self, direction, parent=None):
        super().__init__(parent)
        self.direction = direction
        self.radius = 5
        self.setFlags(QGraphicsItem.GraphicsItemFlag.ItemIsMovable | QGraphicsItem.GraphicsItemFlag.ItemSendsGeometryChanges)

    def boundingRect(self):
        return QRectF(-self.radius, -self.radius, self.radius * 2, self.radius * 2)

    def paint(self, painter, option, widget=None):
        painter.setPen(QPen(QColor(30, 144, 255), 2))
        painter.setBrush(QBrush(QColor(255, 255, 255)))
        painter.drawRect(-self.radius, -self.radius, self.radius * 2, self.radius * 2)

    def mouseMoveEvent(self, event):
        parent = self.parentItem()
        if parent:
            mouse_in_parent = self.mapToParent(event.pos())
            if self.direction == 'W':
                parent.update_size(w=max(20, abs(mouse_in_parent.x()) * 2))
            elif self.direction == 'H':
                parent.update_size(h=max(20, abs(mouse_in_parent.y()) * 2))


class OBBGraphicsItem(QGraphicsObject):
    def __init__(self, cx, cy, w, h, angle, class_id=0, label_name="safety_valve"):
        super().__init__()
        self.w, self.h = w, h
        self.class_id = class_id
        self.label_name = label_name
        self.setPos(cx, cy)
        self.setRotation(angle)
        self.setFlags(QGraphicsItem.ItemIsMovable | QGraphicsItem.ItemIsSelectable)

        self.rot_handle = RotationHandle(self)
        self.w_handle = SizeHandle('W', self)
        self.h_handle = SizeHandle('H', self)
        self.reposition_handles()

    def reposition_handles(self):
        self.rot_handle.setPos(0, -self.h / 2 - 25)
        self.w_handle.setPos(self.w / 2, 0)
        self.h_handle.setPos(0, self.h / 2)

    def update_size(self, w=None, h=None):
        if w is not None: self.w = w
        if h is not None: self.h = h
        self.prepareGeometryChange()
        self.reposition_handles()
        self.update()

    def boundingRect(self):
        pad = 50
        return QRectF(-self.w / 2 - pad, -self.h / 2 - pad, self.w + pad * 2, self.h + pad * 2)

    def paint(self, painter, option, widget=None):
        painter.setRenderHint(QPainter.Antialiasing)
        if self.isSelected():
            pen, brush = QPen(QColor(0, 255, 0), 2), QBrush(QColor(0, 255, 0, 40))
        else:
            pen, brush = QPen(QColor(255, 0, 0), 2), QBrush(QColor(255, 0, 0, 20))

        painter.setPen(pen)
        painter.setBrush(brush)
        painter.drawRect(QRectF(-self.w / 2, -self.h / 2, self.w, self.h))

        painter.setPen(QPen(QColor(255, 255, 255)))
        painter.drawText(int(-self.w / 2), int(-self.h / 2) - 5, f"{self.class_id}: {self.label_name}")

    def get_yolo_parameters(self):
        return self.class_id, self.pos().x(), self.pos().y(), self.w, self.h, self.rotation()


# ==========================================
# 专业级画布视图
# ==========================================
class AdvancedGraphicsView(QGraphicsView):
    def __init__(self, scene, parent=None):
        super().__init__(scene, parent)
        self.setRenderHint(QPainter.Antialiasing)
        self.setTransformationAnchor(QGraphicsView.AnchorUnderMouse)
        self.setResizeAnchor(QGraphicsView.AnchorUnderMouse)
        self.setHorizontalScrollBarPolicy(Qt.ScrollBarAlwaysOff)
        self.setVerticalScrollBarPolicy(Qt.ScrollBarAlwaysOff)
        self._is_dragging = False

    def wheelEvent(self, event):
        if event.angleDelta().y() > 0:
            self.scale(1.15, 1.15)
        else:
            self.scale(1 / 1.15, 1 / 1.15)

    def mousePressEvent(self, event):
        if event.button() == Qt.RightButton:
            self.setDragMode(QGraphicsView.ScrollHandDrag)
            super().mousePressEvent(
                event.__class__(event.type(), event.position(), Qt.LeftButton, Qt.LeftButton, event.modifiers()))
            self._is_dragging = True
        else:
            super().mousePressEvent(event)

    def mouseReleaseEvent(self, event):
        if event.button() == Qt.RightButton and self._is_dragging:
            self.setDragMode(QGraphicsView.NoDrag)
            self._is_dragging = False
        else:
            super().mouseReleaseEvent(event)


# ==========================================
# 核心主窗口：Tab 1 数据管理与手动纠偏
# ==========================================
class DataCorrectionTab(QWidget):
    def __init__(self):
        super().__init__()
        self.current_img_path = None
        self.current_label_path = None
        self.img_w = 0
        self.img_h = 0

        self.init_ui()

    def init_ui(self):
        main_layout = QHBoxLayout(self)

        # ---------------- 左侧：文件列表 ----------------
        left_layout = QVBoxLayout()
        self.btn_open_dir = QPushButton("📂 打开 images 目录")
        self.btn_open_dir.clicked.connect(self.load_directory)
        self.list_widget = QListWidget()
        self.list_widget.itemSelectionChanged.connect(self.on_image_selected)

        left_layout.addWidget(self.btn_open_dir)
        left_layout.addWidget(self.list_widget)
        main_layout.addLayout(left_layout, 2)

        # ---------------- 中间：高精度画布 ----------------
        self.scene = QGraphicsScene(self)
        self.view = AdvancedGraphicsView(self.scene)
        main_layout.addWidget(self.view, 7)

        # ---------------- 右侧：工具栏 ----------------
        right_layout = QVBoxLayout()
        self.lbl_info = QLabel("当前图片: 无\n尺寸: 无")
        self.btn_save = QPushButton("💾 保存修改 (Ctrl+S)")
        self.btn_save.setMinimumHeight(50)
        self.btn_save.setStyleSheet("background-color: #4CAF50; color: white; font-weight: bold;")
        self.btn_save.clicked.connect(self.save_labels)

        self.btn_auto = QPushButton("🤖 自动预标注\n(外挂模型接口)")
        self.btn_auto.setMinimumHeight(50)

        right_layout.addWidget(self.lbl_info)
        right_layout.addStretch()
        right_layout.addWidget(self.btn_auto)
        right_layout.addWidget(self.btn_save)
        main_layout.addLayout(right_layout, 1)

    def load_directory(self):
        from PySide6.QtWidgets import QFileDialog
        dir_path = QFileDialog.getExistingDirectory(self, "选择 images 文件夹")
        if not dir_path: return

        self.list_widget.clear()
        for f in os.listdir(dir_path):
            if f.lower().endswith(('.png', '.jpg', '.jpeg', '.bmp')):
                self.list_widget.addItem(os.path.join(dir_path, f))

    def on_image_selected(self):
        items = self.list_widget.selectedItems()
        if not items: return

        self.current_img_path = items[0].text()
        self.scene.clear()

        # 1. 加载图像底图
        pixmap = QPixmap(self.current_img_path)
        self.img_w, self.img_h = pixmap.width(), pixmap.height()
        self.scene.setSceneRect(0, 0, self.img_w, self.img_h)
        bg_item = QGraphicsPixmapItem(pixmap)
        self.scene.addItem(bg_item)

        self.lbl_info.setText(
            f"当前图片:\n{os.path.basename(self.current_img_path)}\n\n尺寸: {self.img_w} x {self.img_h}")

        # 2. 寻找并解析同名 TXT 标签
        img_dir = os.path.dirname(self.current_img_path)
        # 假设标签存在同级的 labels 文件夹中
        base_dir = os.path.dirname(img_dir)
        filename_no_ext = os.path.splitext(os.path.basename(self.current_img_path))[0]

        self.current_label_path = os.path.join(base_dir, "labels", f"{filename_no_ext}.txt")

        if os.path.exists(self.current_label_path):
            self.load_yolo_labels()

        # 自动缩放画布以适应屏幕
        self.view.fitInView(self.scene.sceneRect(), Qt.KeepAspectRatio)

    def load_yolo_labels(self):
        with open(self.current_label_path, 'r') as f:
            lines = f.readlines()

        for line in lines:
            parts = line.strip().split()
            if len(parts) >= 9:  # class_id + 8个点坐标
                class_id = int(parts[0])
                coords = [float(x) for x in parts[1:9]]

                # 数学转换：归一化坐标 -> cx, cy, w, h, angle
                cx, cy, w, h, angle_deg = yolo_to_obb(coords, self.img_w, self.img_h)

                # 渲染到画面上
                obb_item = OBBGraphicsItem(cx, cy, w, h, angle_deg, class_id=class_id)
                self.scene.addItem(obb_item)

    def save_labels(self):
        if not self.current_img_path: return

        # 确保 labels 文件夹存在
        os.makedirs(os.path.dirname(self.current_label_path), exist_ok=True)

        # 遍历场景中所有的 OBB 图元
        obb_items = [item for item in self.scene.items() if isinstance(item, OBBGraphicsItem)]

        with open(self.current_label_path, 'w') as f:
            for item in obb_items:
                c_id, cx, cy, w, h, angle = item.get_yolo_parameters()
                # 数学反算：cx, cy, w, h, angle -> 归一化 8 顶点坐标
                yolo_pts = obb_to_yolo(cx, cy, w, h, angle, self.img_w, self.img_h)

                # 拼装成 YOLO 格式字符串
                pts_str = " ".join([f"{pt:.6f}" for pt in yolo_pts])
                line = f"{c_id} {pts_str}\n"
                f.write(line)

        print(f"✅ 保存成功: {self.current_label_path}")
        self.lbl_info.setText(self.lbl_info.text() + "\n\n[状态: 已保存]")


# ==========================================
# 启动入口
# ==========================================
class MainWindow(QMainWindow):
    def __init__(self):
        super().__init__()
        self.setWindowTitle("Tab 1: 工业级 OBB 数据集标注与管理模块")
        self.resize(1400, 900)
        self.tab1 = DataCorrectionTab()
        self.setCentralWidget(self.tab1)

    def keyPressEvent(self, event):
        """ 捕捉主窗口全局的 Ctrl+S """
        if event.key() == Qt.Key_S and event.modifiers() == Qt.ControlModifier:
            self.tab1.save_labels()


if __name__ == "__main__":
    app = QApplication(sys.argv)

    # 设置黑色炫酷的全局工业风样式 (可选)
    app.setStyleSheet("""
        QMainWindow, QWidget { background-color: #2b2b2b; color: #ffffff; font-family: 'Segoe UI'; }
        QListWidget { background-color: #1e1e1e; border: 1px solid #3f3f46; font-size: 14px; }
        QPushButton { background-color: #3f3f46; border: none; padding: 10px; border-radius: 4px; }
        QPushButton:hover { background-color: #007acc; }
    """)

    window = MainWindow()
    window.show()
    sys.exit(app.exec())