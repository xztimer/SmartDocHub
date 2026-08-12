export const base64ToFile = (
  base64Data,
  fileName = `${new Date().getTime()}.jpg`,
  mimeType = 'image/jpeg'
) => {
  try {
    if (!base64Data) {
      throw new Error('base64Data 不能为空')
    }

    // 将 Base64 字符串转换为 Uint8Array
    const binaryData = atob(base64Data)
    const arrayBuffer = new ArrayBuffer(binaryData.length)
    const uint8Array = new Uint8Array(arrayBuffer)

    for (let i = 0; i < binaryData.length; i++) {
      uint8Array[i] = binaryData.charCodeAt(i)
    }

    // 创建 Blob 对象
    const blob = new Blob([uint8Array], { type: mimeType })

    // 创建 File 对象
    const file = new File([blob], fileName, { type: mimeType })
    return file
  } catch (error) {
    console.error('Base64转File失败:', error)
    throw error
  }
}

export function parseTime(time, cFormat) {
  if (arguments.length === 0 || !time) {
    return null
  }
  const format = cFormat || '{y}-{m}-{d} {h}:{i}:{s}'
  let date
  if (typeof time === 'object') {
    date = time
  } else {
    if (typeof time === 'string') {
      if (/^[0-9]+$/.test(time)) {
        // support "1548221490638"
        time = parseInt(time)
      } else {
        // support safari
        // https://stackoverflow.com/questions/4310953/invalid-date-in-safari
        time = time.replace(new RegExp(/-/gm), '/')
      }
    }

    if (typeof time === 'number' && time.toString().length === 10) {
      time = time * 1000
    }
    date = new Date(time)
  }
  const formatObj = {
    y: date.getFullYear(),
    m: date.getMonth() + 1,
    d: date.getDate(),
    h: date.getHours(),
    i: date.getMinutes(),
    s: date.getSeconds(),
    a: date.getDay()
  }
  const time_str = format.replace(/{([ymdhisa])+}/g, (result, key) => {
    const value = formatObj[key]
    // Note: getDay() returns 0 on Sunday
    if (key === 'a') {
      return ['日', '一', '二', '三', '四', '五', '六'][value]
    }
    return value.toString().padStart(2, '0')
  })
  return time_str
}

export function listToTree(list) {
  const infoMap = new Map()
  const treeData = []

  list.forEach((item) => {
    infoMap.set(item.id, { ...item, children: [] })
  })

  infoMap.forEach((item) => {
    const parentId = item.parentId
    if (parentId === null || parentId === 0) {
      treeData.push(item)
    } else {
      const parent = infoMap.get(parentId)
      if (parent) {
        parent.children.push(item)
      } else {
        treeData.push(item)
      }
    }
  })

  const sortNodes = (nodes) => {
    nodes.sort((a, b) => a.sort - b.sort)
    nodes.forEach((node) => {
      if (node.children && node.children.length > 0) {
        sortNodes(node.children)
      }
    })
  }
  sortNodes(treeData)

  return treeData
}
