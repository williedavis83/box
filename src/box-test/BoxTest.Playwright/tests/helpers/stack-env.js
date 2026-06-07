export function stackHttp(stackName, logicalName) {
  return process.env[`${stackName.toUpperCase()}_${logicalName.toUpperCase().replace(/-/g, '_')}_HTTP`]
}
