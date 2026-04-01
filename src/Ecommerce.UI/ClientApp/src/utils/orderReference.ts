export function toOrderReference(orderId: string): string {
  const normalizedId = orderId.trim().toUpperCase()

  if (!normalizedId) {
    return 'ORD-UNKNOWN'
  }

  const [prefix] = normalizedId.split('-')
  return `ORD-${prefix}`
}
